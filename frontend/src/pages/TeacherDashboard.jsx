import { useEffect, useRef, useState, Suspense, lazy } from 'react';
import * as api from '../services/api.js';
import { useAuth } from '../contexts/AuthContext.jsx';

export default function TeacherDashboard() {
  const { username } = useAuth();
  const [exercises, setExercises] = useState([]);
  const [groups, setGroups] = useState([]);
  const [students, setStudents] = useState([]);
  const [creatingGroup, setCreatingGroup] = useState({ name: '' });
  const [creatingExercise, setCreatingExercise] = useState({ title: '', description: '', isVisible: true, expectedImageBase64: '', groupIds: [] });
  const [message, setMessage] = useState(null);
  const [loading, setLoading] = useState(true);
  const fileInputRef = useRef(null);
  const MAX_IMAGE_SIZE = 10 * 1024 * 1024; // 10MB
  const [openGroups, setOpenGroups] = useState({}); // { [groupId]: boolean }
  const [exerciseModal, setExerciseModal] = useState({ open: false, loading: false, exerciseId: null, error: null, users: [] });
  const [expandedUser, setExpandedUser] = useState(null); // { username, groupNames, submissions: [] }
  // Submission details modal (reuse student view)
  const [teacherDetailsModal, setTeacherDetailsModal] = useState({ open: false, loading: false, data: null, error: null, id: null });
  const [editorExtensions, setEditorExtensions] = useState([]);
  const [detailsImageUrl, setDetailsImageUrl] = useState(null);
  const detailsImageUrlRef = useRef(null);
  const [detailsDataUrl, setDetailsDataUrl] = useState(null);
  const [detailsImageSrcType, setDetailsImageSrcType] = useState('blob');
  const [detailsImgBg, setDetailsImgBg] = useState('dark');
  const [detailsLeftBoxH, setDetailsLeftBoxH] = useState(null);
  const CodeMirrorLazy = lazy(() => import('@uiw/react-codemirror'));

  useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        // Load user (optional for exercises calc) and global groups/students
        const [user, groupsList, studentsList] = await Promise.all([
          api.getUser(username).catch(() => null),
          api.getAllGroups(),
          api.getAllStudents(),
        ]);

        const groupsDetailed = await Promise.all(groupsList.map(async (g) => {
          try { const gr = await api.getGroup(g.id); return { id: g.id, name: gr?.name || g?.name, membersInGroup: gr?.membersInGroup || [], availableExercises: gr?.availableExercises || [] }; } catch { return { id: g.id, name: g?.name || `Group ${g?.id}`, membersInGroup: [], availableExercises: [] }; }
        }));

        // Build exercises from all groups (unique IDs)
        const exerciseRefsFromGroups = groupsDetailed.flatMap(gr => gr.availableExercises || []);
        const uniqueExerciseIds = Array.from(new Set(exerciseRefsFromGroups.map(r => r.id)));

        // Fallback to user's available exercises if groups yielded none
        const fallbackRefs = (user?.availableExercises || []);
        const ids = uniqueExerciseIds.length > 0 ? uniqueExerciseIds : Array.from(new Set(fallbackRefs.map(r => r.id)));

        const detailed = await Promise.all(ids.map(async (id) => {
          try { const ex = await api.getExercise(id); return { id, title: ex?.title || `Exercise ${id}`, description: ex?.description || '', imageBlobUrl: ex?.imageBlobUrl || null }; } catch { return { id, title: `Exercise ${id}`, description: '', imageBlobUrl: null }; }
        }));

        if (mounted) {
          setExercises(detailed);
          setGroups(groupsDetailed);
          setStudents(studentsList);
        }
      } catch (err) {
        setMessage(err.message || 'Failed to load dashboard');
      } finally {
        if (mounted) setLoading(false);
      }
    })();
    return () => { mounted = false; };
  }, [username]);

  // Load CodeMirror extensions dynamically (for submission details view)
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [{ cpp }, view] = await Promise.all([
          import('@codemirror/lang-cpp'),
          import('@codemirror/view'),
        ]);
        if (!cancelled) setEditorExtensions([cpp(), view.lineNumbers()]);
      } catch {
        // ignore
      }
    })();
    return () => { cancelled = true; };
  }, []);

  async function refresh() {
    // reuse effect logic via explicit call
    setLoading(true);
    setMessage(null);
    try {
      const [user, groupsList, studentsList] = await Promise.all([
        api.getUser(username).catch(() => null),
        api.getAllGroups(),
        api.getAllStudents(),
      ]);
      const groupsDetailed = await Promise.all(groupsList.map(async (g) => {
        try { const gr = await api.getGroup(g.id); return { id: g.id, name: gr?.name || g?.name, membersInGroup: gr?.membersInGroup || [], availableExercises: gr?.availableExercises || [] }; } catch { return { id: g.id, name: g?.name || `Group ${g?.id}`, membersInGroup: [], availableExercises: [] }; }
      }));

      const exerciseRefsFromGroups = groupsDetailed.flatMap(gr => gr.availableExercises || []);
      const uniqueExerciseIds = Array.from(new Set(exerciseRefsFromGroups.map(r => r.id)));
      const fallbackRefs = (user?.availableExercises || []);
      const ids = uniqueExerciseIds.length > 0 ? uniqueExerciseIds : Array.from(new Set(fallbackRefs.map(r => r.id)));

      const detailed = await Promise.all(ids.map(async (id) => {
        try { const ex = await api.getExercise(id); return { id, title: ex?.title || `Exercise ${id}`, description: ex?.description || '', imageBlobUrl: ex?.imageBlobUrl || null }; } catch { return { id, title: `Exercise ${id}`, description: '', imageBlobUrl: null }; }
      }));

      setExercises(detailed);
      setGroups(groupsDetailed);
      setStudents(studentsList);
    } catch (err) {
      setMessage(err.message || 'Failed to refresh');
    } finally {
      setLoading(false);
    }
  }

  async function handleCreateGroup(e) {
    e.preventDefault();
    try {
      await api.createGroup({ groupName: creatingGroup.name, userIds: [] });
      setCreatingGroup({ name: '' });
      setMessage('Group created');
      await refresh();
    } catch (err) {
      setMessage(err.message || 'Failed to create group');
    }
  }

  async function handleCreateExercise(e) {
    e.preventDefault();
    try {
      const { title, description, isVisible, expectedImageBase64, groupIds } = creatingExercise;
      await api.createExercise({ title, description, isVisible, expectedImageBase64, groupIds: groupIds.map(Number).filter(n => !Number.isNaN(n)) });
      setCreatingExercise({ title: '', description: '', isVisible: true, expectedImageBase64: '', groupIds: [] });
      // Clear file input element after successful submit
      if (fileInputRef.current) fileInputRef.current.value = '';
      setMessage('Exercise created');
      await refresh();
    } catch (err) {
      setMessage(err.message || 'Failed to create exercise');
    }
  }

  // Quick header signature checks for common image formats
  function isKnownImageSignature(buf) {
    const bytes = new Uint8Array(buf);
    const startsWith = (sig) => sig.every((b, i) => bytes[i] === b);
    // PNG
    if (startsWith([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])) return true;
    // JPEG
    if (startsWith([0xFF, 0xD8, 0xFF])) return true;
    // GIF87a / GIF89a
    if (startsWith([0x47, 0x49, 0x46, 0x38])) return true;
    // BMP
    if (startsWith([0x42, 0x4D])) return true;
    // WEBP (RIFF....WEBP)
    if (
      bytes.length >= 12 &&
      bytes[0] === 0x52 && bytes[1] === 0x49 && bytes[2] === 0x46 && bytes[3] === 0x46 &&
      bytes[8] === 0x57 && bytes[9] === 0x45 && bytes[10] === 0x42 && bytes[11] === 0x50
    ) return true;
    return false;
  }

  async function handleExpectedImageFileChange(e) {
    const file = e.target.files && e.target.files[0];
    if (!file) return;

    // Basic MIME type check
    if (file.type && !file.type.startsWith('image/')) {
      setMessage('Please choose a valid image file.');
      setCreatingExercise(v => ({ ...v, expectedImageBase64: '' }));
      if (fileInputRef.current) fileInputRef.current.value = '';
      return;
    }

    // Size guard
    if (file.size > MAX_IMAGE_SIZE) {
      setMessage('Image is too large. Please choose a file under 10MB.');
      setCreatingExercise(v => ({ ...v, expectedImageBase64: '' }));
      if (fileInputRef.current) fileInputRef.current.value = '';
      return;
    }

    // Magic header check for robustness
    try {
      const headerBuf = await file.slice(0, 16).arrayBuffer();
      if (!isKnownImageSignature(headerBuf)) {
        setMessage('Selected file does not appear to be a valid image.');
        setCreatingExercise(v => ({ ...v, expectedImageBase64: '' }));
        if (fileInputRef.current) fileInputRef.current.value = '';
        return;
      }
    } catch (err) {
      // Non-fatal; continue to attempt reading as Data URL
    }

    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result || '';
      // result is usually a data URL: data:image/<type>;base64,<base64>
      let base64 = '';
      const marker = 'base64,';
      const idx = typeof result === 'string' ? result.indexOf(marker) : -1;
      if (idx !== -1) {
        base64 = result.substring(idx + marker.length);
      } else if (typeof result === 'string') {
        // Fallback: if it's already base64 without data URI
        base64 = result;
      }
      setCreatingExercise(v => ({ ...v, expectedImageBase64: base64 }));
    };
    reader.onerror = () => {
      setMessage('Failed to read image file');
      setCreatingExercise(v => ({ ...v, expectedImageBase64: '' }));
      if (fileInputRef.current) fileInputRef.current.value = '';
    };
    reader.readAsDataURL(file);
  }

  async function handleAssignStudentToGroup(e) {
    e.preventDefault();
    const form = e.currentTarget;
    const student = form.student.value;
    const group = form.group.value;
    if (!student || !group) return;
    try {
      await api.assignUserToGroup(Number(group), Number(student));
      setMessage('Student assigned to group');
      await refresh();
      form.reset();
    } catch (err) {
      setMessage(err.message || 'Failed to assign student');
    }
  }

  async function handleAssignExerciseToGroup(e) {
    e.preventDefault();
    const form = e.currentTarget;
    const exercise = form.exercise.value;
    const group = form.group.value;
    if (!exercise || !group) return;
    try {
      await api.assignExerciseToGroup(Number(exercise), Number(group));
      setMessage('Exercise assigned to group');
      await refresh();
      form.reset();
    } catch (err) {
      setMessage(err.message || 'Failed to assign exercise');
    }
  }

  function toggleGroupOpen(id) {
    setOpenGroups(prev => ({ ...prev, [id]: !prev[id] }));
  }

  function getExerciseTitleById(id) {
    const ex = exercises.find(e => e.id === id);
    return ex?.title || `Exercise ${id}`;
  }

  async function openExerciseDetails(exerciseId) {
    try {
      // Show modal immediately with loading state
  setExerciseModal({ open: true, loading: true, exerciseId, error: null, users: [] });
  setExpandedUser(null);

      // Determine which groups include this exercise
      const groupsWithExercise = groups.filter(g => Array.isArray(g?.availableExercises) && g.availableExercises.some(r => r?.id === exerciseId));

      // Build user -> groups map
      const userMap = new Map(); // key: username or id; value: { id, username, groups: Set<string> }
      for (const g of groupsWithExercise) {
        const gName = g?.name || `Group ${g?.id}`;
        const members = Array.isArray(g?.membersInGroup) ? g.membersInGroup : [];
        for (const m of members) {
          const key = (m?.username || m?.name || m?.id || '').toString();
          if (!key) continue;
          if (!userMap.has(key)) {
            userMap.set(key, { id: m?.id ?? null, username: m?.username || m?.name || key, groups: new Set() });
          }
          userMap.get(key).groups.add(gName);
        }
      }

      // Fetch exercise details to get submission IDs
    const exerciseDetails = await api.getExercise(exerciseId);
    const submissionIdSet = new Set((Array.isArray(exerciseDetails?.submissions) ? exerciseDetails.submissions : [])
        .map(r => r?.id)
        .filter(v => v != null));

    // For each user, fetch their submissions and include filtered ones for this exercise
      const usersArr = Array.from(userMap.values());
    const enriched = await Promise.all(usersArr.map(async (u) => {
        try {
          const subs = await api.getUserSubmissions(u.username);
      const filtered = (subs || []).filter(s => submissionIdSet.has(s?.id));
      const hasHighScore = filtered.some(s => typeof s?.score === 'number' && s.score > 0.85);
      return { ...u, groupNames: Array.from(u.groups), submissionCount: filtered.length, submissions: filtered, hasHighScore };
        } catch (e) {
      return { ...u, groupNames: Array.from(u.groups), submissionCount: 0, submissions: [], hasHighScore: false };
        }
      }));

      // Sort by username
      enriched.sort((a, b) => a.username.localeCompare(b.username));

      setExerciseModal({ open: true, loading: false, exerciseId, error: null, users: enriched });
    } catch (e) {
      setExerciseModal({ open: true, loading: false, exerciseId, error: e?.message || 'Failed to load exercise details', users: [] });
    }
  }

  async function refreshExerciseDetails() {
    if (!exerciseModal?.exerciseId) return;
    const prevUser = expandedUser?.username;
    await openExerciseDetails(exerciseModal.exerciseId);
    if (prevUser) {
      // Reselect previously expanded user if still present
      const u = (exerciseModal?.users || []).find(x => x.username === prevUser);
      if (u) setExpandedUser(u);
    }
  }

  function onUserRowClick(u) {
    setExpandedUser(prev => (prev && prev.username === u.username ? null : u));
  }

  async function openTeacherSubmissionDetails(submission) {
    const numericId = submission?.id;
    const subIdStr = numericId != null ? String(numericId) : '';
    setTeacherDetailsModal({ open: true, loading: true, data: null, error: null, id: subIdStr });
    try {
      const details = await api.getSubmissionStatus(subIdStr || numericId);
      setTeacherDetailsModal({ open: true, loading: false, data: details, error: null, id: subIdStr });
    } catch (e) {
      setTeacherDetailsModal({ open: true, loading: false, data: null, error: e?.message || 'Failed to load submission', id: subIdStr });
    }
  }

  // Manage result image URL lifecycle for teacherDetailsModal
  useEffect(() => {
    const hasData = teacherDetailsModal?.open && teacherDetailsModal?.data;
    const newUrl = hasData ? resultImageDataUrl(teacherDetailsModal.data) : null;
    const newDataUrl = hasData ? resultImageDataUri(teacherDetailsModal.data) : null;
    setDetailsImageUrl(newUrl || null);
    setDetailsDataUrl(newDataUrl || null);
    setDetailsImageSrcType('blob');
    setDetailsLeftBoxH(null);
    const oldUrl = detailsImageUrlRef.current;
    if (oldUrl && oldUrl !== newUrl && typeof oldUrl === 'string' && oldUrl.startsWith('blob:')) {
      try { URL.revokeObjectURL(oldUrl); } catch {}
    }
    detailsImageUrlRef.current = newUrl || null;
    return () => {
      const curr = detailsImageUrlRef.current;
      if (curr && typeof curr === 'string' && curr.startsWith('blob:')) {
        try { URL.revokeObjectURL(curr); } catch {}
      }
      detailsImageUrlRef.current = null;
    };
  }, [teacherDetailsModal.open, teacherDetailsModal.data]);

  return (
    <div className="pb-4">
      <h2 className="h4 mb-3">Teacher Dashboard</h2>
      {message && <div className="alert alert-info py-2">{message}</div>}
      {loading ? (
        <div className="text-muted">Loading…</div>
      ) : (
        <div className="row g-4">
          <div className="col-12 col-lg-6">
            <div className="card h-100">
              <div className="card-body">
                <h3 className="h5 mb-3">Exercises you manage</h3>
                <div className="list-group mb-3">
                  {exercises.map(ex => (
                    <div
                      key={ex.id}
                      className="list-group-item list-group-item-action d-flex align-items-center gg-clickable"
                      role="button"
                      tabIndex={0}
                      onClick={() => openExerciseDetails(ex.id)}
                      onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openExerciseDetails(ex.id); } }}
                      title="Click to view details"
                    >
                      {ex.imageBlobUrl ? (
                        <img
                          src={ex.imageBlobUrl}
                          alt={`${ex.title} thumbnail`}
                          className="gg-thumb-sm me-3"
                        />
                      ) : (
                        <div
                          className="me-3 rounded bg-light border"
                          style={{ width: 80, height: 48 }}
                          aria-hidden="true"
                        />
                      )}
                      <div className="flex-grow-1">
                        <div className="fw-semibold">{ex.title}</div>
                        {ex.description && <div className="text-muted small">{ex.description}</div>}
                      </div>
                    </div>
                  ))}
                  {exercises.length === 0 && <div className="text-muted">No exercises yet.</div>}
                </div>

                <form onSubmit={handleCreateExercise} className="border-top pt-3">
                  <h4 className="h6">Create Exercise</h4>
                  <div className="mb-2">
                    <input className="form-control" placeholder="Title" value={creatingExercise.title} onChange={(e) => setCreatingExercise(v => ({ ...v, title: e.target.value }))} required />
                  </div>
                  <div className="mb-2">
                    <textarea className="form-control" placeholder="Description" rows="2" value={creatingExercise.description} onChange={(e) => setCreatingExercise(v => ({ ...v, description: e.target.value }))} />
                  </div>
                  <div className="form-check form-switch mb-2">
                    <input className="form-check-input" type="checkbox" role="switch" id="exVisible" checked={creatingExercise.isVisible} onChange={(e) => setCreatingExercise(v => ({ ...v, isVisible: e.target.checked }))} />
                    <label className="form-check-label" htmlFor="exVisible">Visible to students</label>
                  </div>
                  <div className="mb-2">
                    <label className="form-label">Expected Image (upload)</label>
                    <input
                      ref={fileInputRef}
                      type="file"
                      className="form-control"
                      accept="image/*"
                      onChange={handleExpectedImageFileChange}
                      required
                    />
                  </div>
                  <div className="mb-2">
                    <label className="form-label">Assign to Groups</label>
                    <select multiple className="form-select" value={creatingExercise.groupIds} onChange={(e) => {
                      const selected = Array.from(e.target.selectedOptions).map(o => o.value);
                      setCreatingExercise(v => ({ ...v, groupIds: selected }));
                    }}>
                      {groups.map(g => (
                        <option key={g.id} value={g.id}>{g.name}</option>
                      ))}
                    </select>
                    <div className="form-text">Hold Ctrl/Command to select multiple groups.</div>
                  </div>
                  <button className="btn btn-primary">Create</button>
                </form>
              </div>
            </div>
          </div>

          <div className="col-12 col-lg-6">
            <div className="card h-100">
              <div className="card-body">
                <h3 className="h5 mb-3">Your Groups</h3>
                <div className="list-group mb-3">
                  {groups.map(g => (
                    <div key={g.id} className="list-group-item">
                      <button
                        type="button"
                        className="btn btn-link text-decoration-none p-0 w-100 d-flex justify-content-between align-items-center"
                        onClick={() => toggleGroupOpen(g.id)}
                        aria-expanded={!!openGroups[g.id]}
                        aria-controls={`group-${g.id}-details`}
                      >
                        <div>
                          <div className="fw-semibold mb-1">{g.name}</div>
                          <div className="small text-muted">Members: {g.membersInGroup?.length ?? 0} • Exercises: {g.availableExercises?.length ?? 0}</div>
                        </div>
                        <span className="ms-2" aria-hidden="true">{openGroups[g.id] ? '▾' : '▸'}</span>
                      </button>

                      {openGroups[g.id] && (
                        <div id={`group-${g.id}-details`} className="mt-2 border-top pt-2">
                          <div className="row g-3">
                            <div className="col-12 col-md-6">
                              <div className="small text-uppercase text-muted mb-1">Members</div>
                              {Array.isArray(g.membersInGroup) && g.membersInGroup.length > 0 ? (
                                <ul className="mb-0 ps-3 small">
                                  {g.membersInGroup.map((m, idx) => (
                                    <li key={m?.id ?? idx}>{m?.username || m?.name || `User ${m?.id ?? ''}`}</li>
                                  ))}
                                </ul>
                              ) : (
                                <div className="text-muted small">No members.</div>
                              )}
                            </div>
                            <div className="col-12 col-md-6">
                              <div className="small text-uppercase text-muted mb-1">Exercises</div>
                              {Array.isArray(g.availableExercises) && g.availableExercises.length > 0 ? (
                                <ul className="mb-0 ps-3 small">
                                  {Array.from(new Set(g.availableExercises.map(r => r?.id))).map((id, idx) => (
                                    <li key={id ?? idx}>{getExerciseTitleById(id)}</li>
                                  ))}
                                </ul>
                              ) : (
                                <div className="text-muted small">No exercises.</div>
                              )}
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                  ))}
                  {groups.length === 0 && <div className="text-muted">No groups yet.</div>}
                </div>

                <form onSubmit={handleCreateGroup} className="border-top pt-3">
                  <h4 className="h6">Create Group</h4>
                  <div className="mb-2">
                    <input className="form-control" placeholder="Group name" value={creatingGroup.name} onChange={(e) => setCreatingGroup({ name: e.target.value })} required />
                  </div>
                  <button className="btn btn-outline-primary">Create Group</button>
                </form>

                <form onSubmit={handleAssignStudentToGroup} className="border-top pt-3 mt-3">
                  <h4 className="h6">Assign Student to Group</h4>
                  <div className="row g-2 align-items-end">
                    <div className="col">
                      <label className="form-label">Student</label>
                      <select name="student" className="form-select" required>
                        <option value="">Select student…</option>
                        {students.map(s => (
                          <option key={s.id} value={s.id}>{s.username}</option>
                        ))}
                      </select>
                    </div>
                    <div className="col">
                      <label className="form-label">Group</label>
                      <select name="group" className="form-select" required>
                        <option value="">Select group…</option>
                        {groups.map(g => (
                          <option key={g.id} value={g.id}>{g.name}</option>
                        ))}
                      </select>
                    </div>
                    <div className="col-12">
                      <button className="btn btn-outline-primary">Assign Student</button>
                    </div>
                  </div>
                </form>

                <form onSubmit={handleAssignExerciseToGroup} className="border-top pt-3 mt-3">
                  <h4 className="h6">Assign Exercise to Group</h4>
                  <div className="row g-2 align-items-end">
                    <div className="col">
                      <label className="form-label">Exercise</label>
                      <select name="exercise" className="form-select" required>
                        <option value="">Select exercise…</option>
                        {exercises.map(ex => (
                          <option key={ex.id} value={ex.id}>{ex.title}</option>
                        ))}
                      </select>
                    </div>
                    <div className="col">
                      <label className="form-label">Group</label>
                      <select name="group" className="form-select" required>
                        <option value="">Select group…</option>
                        {groups.map(g => (
                          <option key={g.id} value={g.id}>{g.name}</option>
                        ))}
                      </select>
                    </div>
                    <div className="col-12">
                      <button className="btn btn-outline-primary">Assign Exercise</button>
                    </div>
                  </div>
                </form>
              </div>
            </div>
          </div>
        </div>
      )}
      {/* Exercise details modal */}
      {exerciseModal.open && (
        <>
          <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1" role="dialog" aria-modal="true">
            <div className="modal-dialog modal-lg modal-dialog-scrollable">
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title">Exercise: {getExerciseTitleById(exerciseModal.exerciseId)}</h5>
                  <button type="button" className="btn-close" aria-label="Close" onClick={() => setExerciseModal({ open: false, loading: false, exerciseId: null, error: null, users: [] })}></button>
                </div>
                <div className="modal-body">
                  {exerciseModal.loading && <div className="text-muted">Loading…</div>}
                  {exerciseModal.error && <div className="alert alert-danger py-2">{exerciseModal.error}</div>}
                  {!exerciseModal.loading && !exerciseModal.error && (
                    <div>
                      {exerciseModal.users.length === 0 ? (
                        <div className="text-muted">No users have this exercise assigned.</div>
                      ) : (
                        <div className="table-responsive">
                          <table className="table table-sm align-middle mb-0">
                            <thead>
                              <tr>
                                <th>User</th>
                                <th>Group(s)</th>
                                <th className="text-end">Submissions</th>
                              </tr>
                            </thead>
                            <tbody>
                {exerciseModal.users.map(u => (
                                <tr
                                  key={u.username || u.id}
                  className={`gg-clickable ${u.hasHighScore ? 'table-success' : ''}`}
                                  role="button"
                                  tabIndex={0}
                                  onClick={() => onUserRowClick(u)}
                                  onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onUserRowClick(u); } }}
                                  title="Click to view submissions"
                                >
                                  <td>{u.username}</td>
                                  <td>{(u.groupNames || []).join(', ')}</td>
                                  <td className="text-end"><span className="badge bg-primary">{u.submissionCount ?? 0}</span></td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>
                      )}
                      {expandedUser && (
                        <div className="mt-3">
                          <div className="d-flex align-items-center justify-content-between">
                            <h6 className="mb-2">Submissions for {expandedUser.username}</h6>
                          </div>
                          {(expandedUser.submissions?.length ?? 0) > 0 ? (
                            <div className="table-responsive">
                              <table className="table table-sm table-hover align-middle mb-0">
                                <thead>
                                  <tr>
                                    <th>ID</th>
                                    <th>Status</th>
                                    <th>Score</th>
                                    <th>Submitted</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {expandedUser.submissions.map((s) => (
                                    <tr
                                      key={s.id}
                                      className="gg-submission-row"
                                      onClick={() => openTeacherSubmissionDetails(s)}
                                      role="button"
                                      tabIndex={0}
                                      onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openTeacherSubmissionDetails(s); } }}
                                      title="Click to view details"
                                    >
                                      <td>{s.id}</td>
                                      <td>{statusBadge(s.status)}</td>
                                      <td>{renderScoreBadge(s.score)}</td>
                                      <td>{s.submittedAt ? new Date(s.submittedAt).toLocaleString() : ''}</td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                          ) : (
                            <div className="text-muted">No submissions for this user.</div>
                          )}
                        </div>
                      )}
                    </div>
                  )}
                </div>
                <div className="modal-footer">
                  <button type="button" className="btn btn-outline-secondary" onClick={refreshExerciseDetails} disabled={!!exerciseModal.loading}>Refresh</button>
                  <button type="button" className="btn btn-primary" onClick={() => setExerciseModal({ open: false, loading: false, exerciseId: null, error: null, users: [] })}>Close</button>
                </div>
              </div>
            </div>
          </div>
          <div className="modal-backdrop fade show" onClick={() => setExerciseModal({ open: false, loading: false, exerciseId: null, error: null, users: [] })}></div>
        </>
      )}
      {renderDetailsModal(
        teacherDetailsModal,
        () => setTeacherDetailsModal({ open: false, loading: false, data: null, error: null, id: null }),
        detailsImageUrl,
        detailsDataUrl,
        detailsImageSrcType,
        setDetailsImageSrcType,
        detailsImgBg,
        setDetailsImgBg,
        editorExtensions,
        detailsLeftBoxH,
        setDetailsLeftBoxH,
        CodeMirrorLazy,
        Suspense
      )}
    </div>
  );
}

// ===== Helpers reused from StudentDashboard.jsx =====
function statusBadge(status) {
  const label = renderStatusText(status);
  const cls =
    status === 0 ? 'bg-secondary' : // NotQueued
    status === 1 ? 'bg-info' :      // Queued
    status === 2 ? 'bg-primary' :   // Running
    status === 3 ? 'bg-success' :   // Finished
    'bg-light text-dark';
  return <span className={`badge rounded-pill ${cls}`}>{label}</span>;
}

function renderStatusText(status) {
  switch (status) {
    case 0: return 'NotQueued';
    case 1: return 'Queued';
    case 2: return 'Running';
    case 3: return 'Finished';
    default: return String(status);
  }
}

function scoreColorRGB(score) {
  if (score == null || Number.isNaN(score)) return null;
  const t = Math.max(0, Math.min(1, score));
  const r1 = 0xdc, g1 = 0x35, b1 = 0x45; // red
  const r2 = 0x19, g2 = 0x87, b2 = 0x54; // green
  return {
    r: Math.round(r1 + (r2 - r1) * t),
    g: Math.round(g1 + (g2 - g1) * t),
    b: Math.round(b1 + (b2 - b1) * t),
  };
}

function rgbToCss(rgb) {
  if (!rgb) return 'inherit';
  const { r, g, b } = rgb; return `rgb(${r}, ${g}, ${b})`;
}

function readableTextColor(rgb) {
  if (!rgb) return 'inherit';
  const { r, g, b } = rgb;
  const yiq = (r * 299 + g * 587 + b * 114) / 1000;
  return yiq >= 140 ? '#000' : '#fff';
}

function renderScoreBadge(score) {
  if (score == null || Number.isNaN(score)) return <span className="text-muted">-</span>;
  const rgb = scoreColorRGB(score);
  const bg = rgbToCss(rgb);
  const fg = readableTextColor(rgb);
  const label = `${(score * 100).toFixed(0)}%`;
  return (
    <span
      className="badge rounded-pill fw-semibold"
      style={{ backgroundColor: bg, color: fg }}
      title={score.toFixed(3)}
    >
      {label}
    </span>
  );
}

function renderDetailsModal(modal, onClose, imageUrl, dataUrl, srcType, setSrcType, imgBg, setImgBg, editorExtensions, detailsLeftBoxH, setDetailsLeftBoxH, CodeMirrorLazy, SuspenseComp) {
  if (!modal?.open) return null;
  const d = modal.data;
  const chosenUrl = srcType === 'data' ? (dataUrl || imageUrl) : (imageUrl || dataUrl);
  const img = chosenUrl || (d ? resultImageDataUrl(d) : null);
  const acc = d?.submissionResult?.executionAccuracy;
  const code = getSubmissionResultCode(d);
  const srcB64 = d?.sourceCodeBase64 || d?.SourceCodeBase64;
  const sourceCode = typeof srcB64 === 'string' && srcB64.trim().length > 0 ? base64ToUtf8(srcB64) : '';
  const editorHeightPx = Math.max(420, (detailsLeftBoxH ? Math.max(360, detailsLeftBoxH - 36) : 560));
  return (
    <>
      <div className="modal fade show" style={{ display: 'block' }} tabIndex="-1" role="dialog" aria-modal="true">
        <div className="modal-dialog modal-xl modal-dialog-scrollable">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Submission {d?.submissionId || modal.id}</h5>
              <button type="button" className="btn-close" aria-label="Close" onClick={onClose}></button>
            </div>
            <div className="modal-body">
              {modal.loading && <div className="text-muted">Loading…</div>}
              {modal.error && <div className="alert alert-danger py-2">{modal.error}</div>}
              {d && (
                <div>
                  <div className="d-flex flex-wrap gap-2 align-items-center mb-3">
                    <div>Status: {statusBadge(d.status)}</div>
                    {typeof acc === 'number' && <div>Score: {renderScoreBadge(acc)}</div>}
                    {code != null && <div>Error code: {renderErrorCodeBadge(code)}</div>}
                    <div className="text-muted small">Submitted: {formatDateTime(d.submittedAt)}</div>
                    {d.lastUpdate && <div className="text-muted small">Last update: {formatDateTime(d.lastUpdate)}</div>}
                  </div>
                  {d.errorDetails && <div className="alert alert-warning py-2 mb-3">{d.errorDetails}</div>}
                  <div className="row g-3 align-items-start">
                    <div className="col-12 col-lg-6">
                      {img ? (
                        <div className="text-center gg-left-box">
                          <div className={`d-inline-block p-2 rounded ${imgBg === 'dark' ? 'gg-img-bg-dark' : 'gg-img-bg-none'}`}>
                            <img
                              src={img}
                              alt="submission result"
                              className="gg-image-full"
                              onError={(e) => {
                                if (setSrcType && srcType === 'blob') {
                                  setSrcType('data');
                                  e.currentTarget.style.display = 'none';
                                  e.currentTarget.parentElement.insertAdjacentHTML('beforeend', '<div class="text-muted">Retrying with data URI…</div>');
                                } else {
                                  e.currentTarget.style.display = 'none';
                                  e.currentTarget.parentElement.insertAdjacentHTML('beforeend', '<div class="text-muted">Unable to display result image.</div>');
                                }
                              }}
                              onLoad={(e) => {
                                try {
                                  const box = e.currentTarget.closest('.gg-left-box');
                                  if (box) setDetailsLeftBoxH(box.offsetHeight);
                                } catch {}
                              }}
                            />
                          </div>
                          <div className="mt-2 d-flex align-items-center gap-2 justify-content-center">
                            <span className="text-muted small">Background:</span>
                            <div className="btn-group btn-group-sm" role="group" aria-label="Image background">
                              <button type="button" className={`btn ${imgBg==='none' ? 'btn-primary' : 'btn-outline-primary'}`} onClick={() => setImgBg('none')}>None</button>
                              <button type="button" className={`btn ${imgBg==='dark' ? 'btn-primary' : 'btn-outline-primary'}`} onClick={() => setImgBg('dark')}>Dark</button>
                            </div>
                          </div>
                        </div>
                      ) : (
                        <div className="text-muted">No result image available.</div>
                      )}
                    </div>
                    <div className="col-12 col-lg-6">
                      <div className="d-flex align-items-center justify-content-between mb-2">
                        <h6 className="mb-0">Source code</h6>
                      </div>
                      <div className="border rounded overflow-hidden" style={{ height: `${editorHeightPx}px` }}>
                        {sourceCode ? (
                          <SuspenseComp fallback={<div className="p-3 text-muted">Loading viewer…</div>}>
                            <CodeMirrorLazy
                              value={sourceCode}
                              height={`${editorHeightPx}px`}
                              extensions={editorExtensions}
                              editable={false}
                            />
                          </SuspenseComp>
                        ) : (
                          <div className="p-3 text-muted">No source code available.</div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop fade show" onClick={onClose}></div>
    </>
  );
}

function formatDateTime(s) {
  if (!s) return '';
  try { return new Date(s).toLocaleString(); } catch { return String(s); }
}

function resultImageDataUrl(details) {
  if (!details) return null;
  const sr = details.submissionResult || details.SubmissionResult || details;
  const candidates = [
    sr?.executionResultBase64,
    sr?.ExecutionResultBase64,
    sr?.resultImageBase64,
    sr?.imageBase64,
    details?.executionResultBase64,
    details?.ExecutionResultBase64,
    details?.SubmissionResult?.ExecutionResultBase64,
    details?.SubmissionResult?.executionResultBase64,
    details?.submissionResult?.ExecutionResultBase64,
  ];
  let payload = candidates.find((v) => v != null);
  const mimeType = sr?.mimeType || sr?.contentType || guessMimeFromPayload(payload);

  if (typeof payload === 'string') {
    const trimmed = payload.trim();
    if (trimmed.startsWith('data:')) return trimmed;
    const clean = trimmed.replace(/^\"|\"$/g, '').replace(/\s+/g, '');
    const type = mimeType || 'image/png';
    try {
      const bytes = base64ToUint8(clean);
      if (bytes && bytes.length > 0) {
        const blob = new Blob([bytes], { type });
        return URL.createObjectURL(blob);
      }
    } catch {}
    return `data:${type};base64,${clean}`;
  }

  const bufCandidates = [
    sr?.executionResultBytes,
    sr?.imageBytes,
    details?.executionResultBytes,
    (payload && payload.type === 'Buffer' && Array.isArray(payload.data)) ? payload.data : null,
  ];
  const bytesArr = bufCandidates.find((v) => Array.isArray(v));
  if (Array.isArray(bytesArr)) {
    try {
      const u8 = new Uint8Array(bytesArr);
      const blob = new Blob([u8], { type: mimeType || 'image/png' });
      return URL.createObjectURL(blob);
    } catch {
      return null;
    }
  }
  return null;
}

function resultImageDataUri(details) {
  if (!details) return null;
  const sr = details.submissionResult || details.SubmissionResult || details;
  const candidates = [
    sr?.executionResultBase64,
    sr?.ExecutionResultBase64,
    sr?.resultImageBase64,
    sr?.imageBase64,
    details?.executionResultBase64,
    details?.ExecutionResultBase64,
    details?.SubmissionResult?.ExecutionResultBase64,
    details?.SubmissionResult?.executionResultBase64,
    details?.submissionResult?.ExecutionResultBase64,
  ];
  let payload = candidates.find((v) => v != null);
  const mimeType = sr?.mimeType || sr?.contentType || guessMimeFromPayload(payload) || 'image/png';
  if (typeof payload === 'string') {
    const trimmed = payload.trim();
    if (trimmed.startsWith('data:')) return trimmed;
    const clean = trimmed.replace(/^\"|\"$/g, '').replace(/\s+/g, '');
    return `data:${mimeType};base64,${clean}`;
  }
  const bufCandidates = [sr?.executionResultBytes, sr?.imageBytes];
  const bytesArr = bufCandidates.find((v) => Array.isArray(v));
  if (Array.isArray(bytesArr)) {
    try {
      const u8 = new Uint8Array(bytesArr);
      let binary = '';
      for (let i = 0; i < u8.length; i++) binary += String.fromCharCode(u8[i]);
      const b64 = btoa(binary);
      return `data:${mimeType};base64,${b64}`;
    } catch {}
  }
  return null;
}

function guessMimeFromPayload(v) {
  if (!v) return null;
  if (typeof v === 'string') {
    const s = v.trim();
    if (s.startsWith('data:')) {
      const m = s.slice(5).split(';', 1)[0];
      return m || null;
    }
    const head = s.slice(0, 8);
    if (head.startsWith('iVBOR')) return 'image/png';
    if (head.startsWith('/9j/')) return 'image/jpeg';
    if (head.startsWith('R0lG')) return 'image/gif';
    if (head.startsWith('Qk0') || head.startsWith('QkF')) return 'image/bmp';
    if (s.startsWith('PHN2')) return 'image/svg+xml';
  }
  return null;
}

function base64ToUint8(b64) {
  const norm = b64.replace(/-/g, '+').replace(/_/g, '/');
  const padded = norm + '==='.slice((norm.length + 3) % 4);
  const bin = atob(padded);
  const len = bin.length;
  const bytes = new Uint8Array(len);
  for (let i = 0; i < len; i++) bytes[i] = bin.charCodeAt(i);
  return bytes;
}

function base64ToUtf8(b64) {
  if (!b64) return '';
  try {
    const norm = b64.replace(/-/g, '+').replace(/_/g, '/');
    const padded = norm + '==='.slice((norm.length + 3) % 4);
    const bin = atob(padded);
    try {
      return decodeURIComponent(escape(bin));
    } catch {
      return bin;
    }
  } catch {
    return '';
  }
}

function getSubmissionResultCode(details) {
  if (!details) return null;
  const sr = details.submissionResult || {};
  const candidates = [sr.errorCode, sr.resultCode, details.errorCode, details.resultCode];
  for (const v of candidates) {
    if (typeof v === 'number') return v;
    const n = v != null ? Number(v) : NaN;
    if (!Number.isNaN(n)) return n;
  }
  return null;
}

function errorCodeText(code) {
  const names = [
    'None',
    'UnknownProcessingError',
    'ExceedsSizeLimits',
    'InvalidImage',
    'InputValidationError',
    'SubmissionNotFound',
    'FlaggedAsSuspicious',
    'CompilationFailed',
    'ExecutionFailed',
    'CapturingError',
    'UnknownExecutionError',
  ];
  if (typeof code === 'number' && code >= 0 && code < names.length) return names[code];
  return String(code);
}

function renderErrorCodeBadge(code) {
  if (code == null) return null;
  const label = errorCodeText(code);
  const cls = code === 0 ? 'bg-secondary' : 'bg-danger';
  return <span className={`badge rounded-pill ${cls}`} title={`Code ${code}`}>{label}</span>;
}
