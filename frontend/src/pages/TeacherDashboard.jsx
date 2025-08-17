import { useEffect, useRef, useState } from 'react';
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
                    <div key={ex.id} className="list-group-item d-flex justify-content-between align-items-start">
                      <div>
                        <div className="fw-semibold">{ex.title}</div>
                        {ex.description && <div className="text-muted small">{ex.description}</div>}
                      </div>
                      {ex.imageBlobUrl && <img src={ex.imageBlobUrl} alt="expected" style={{ maxHeight: 40 }} />}
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
    </div>
  );
}
