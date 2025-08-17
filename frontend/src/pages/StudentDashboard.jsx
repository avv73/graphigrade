import { useEffect, useMemo, useRef, useState, Suspense, lazy } from 'react';
import * as api from '../services/api.js';
import { useAuth } from '../contexts/AuthContext.jsx';

// Lazy-load the heavy editor component to split chunks
const CodeMirrorLazy = lazy(() => import('@uiw/react-codemirror'));

export default function StudentDashboard() {
  const { username } = useAuth();
  const [exercises, setExercises] = useState([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const pollRef = useRef(null);
  const [codeByExercise, setCodeByExercise] = useState({});
  const [activeId, setActiveId] = useState(null);
  const [submissionsByExercise, setSubmissionsByExercise] = useState({}); // { [exerciseId]: Array<{ id, status, score, submittedAt }> }
  const [userSubmissions, setUserSubmissions] = useState([]);
  const [exerciseSubmissionIds, setExerciseSubmissionIds] = useState({}); // { [exerciseId]: Set<number> }
  const [exerciseSubmissionRefs, setExerciseSubmissionRefs] = useState({}); // { [exerciseId]: { [id:number]: ref } }
  const [detailsModal, setDetailsModal] = useState({ open: false, loading: false, data: null, error: null, id: null });
  const [editorExtensions, setEditorExtensions] = useState([]);
  const [refreshing, setRefreshing] = useState({}); // { [exerciseId]: boolean }
  // Track a computed image URL for the details modal and revoke blob URLs on change/close
  const [detailsImageUrl, setDetailsImageUrl] = useState(null);
  const detailsImageUrlRef = useRef(null);
  const [detailsDataUrl, setDetailsDataUrl] = useState(null); // data: URI fallback
  const [detailsImageSrcType, setDetailsImageSrcType] = useState('blob'); // 'blob' | 'data'
  const [detailsImgBg, setDetailsImgBg] = useState('dark'); // 'none' | 'dark'
  const [detailsLeftBoxH, setDetailsLeftBoxH] = useState(null); // measured px height of image box

  useEffect(() => {
    let isMounted = true;
    api.fetchStudentExercises(username)
      .then((data) => { if (isMounted) { setExercises(data); if (data?.length && !activeId) setActiveId(data[0].id); } })
      .catch(() => { if (isMounted) setExercises([]); })
      .finally(() => setLoading(false));
    return () => { isMounted = false; };
  }, [username]);

  // Load CodeMirror extensions dynamically to further split chunks
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

  // Load user submissions initially
  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await api.getUserSubmissions(username);
        if (!cancelled) setUserSubmissions(data ?? []);
      } catch (e) {
        if (!cancelled) setError(e.message || 'Failed to load submissions');
      }
    })();
    return () => { cancelled = true; };
  }, [username]);

  // When active exercise changes, load its submission IDs and filter user's submissions
  useEffect(() => {
    if (!activeId) return;
    let cancelled = false;
    (async () => {
      try {
        const ex = await api.getExercise(activeId);
        const refs = Array.isArray(ex?.submissions) ? ex.submissions : [];
        const ids = new Set(refs.map(r => r?.id).filter((x) => x != null));
        if (!cancelled) {
          setExerciseSubmissionIds(prev => ({ ...prev, [activeId]: ids }));
          // Build ref map for extracting string submission ids later
          const map = {};
          refs.forEach((r) => { if (r && r.id != null) map[r.id] = r; });
          setExerciseSubmissionRefs(prev => ({ ...prev, [activeId]: map }));
          const filtered = (userSubmissions || []).filter(s => ids.has(s.id));
          setSubmissionsByExercise(prev => ({ ...prev, [activeId]: filtered }));
        }
      } catch (e) {
        if (!cancelled) setError(e.message || 'Failed to load exercise submissions');
      }
    })();
    return () => { cancelled = true; };
  }, [activeId]);

  // Poll user's submissions globally while any are in progress; re-filter current exercise on change
  // In-progress statuses: Queued (1) or Running (2)
  const hasInProgress = useMemo(() => (userSubmissions || []).some(s => s?.status === 1 || s?.status === 2), [userSubmissions]);
  useEffect(() => {
    if (!hasInProgress) { if (pollRef.current) clearInterval(pollRef.current); return; }
    if (pollRef.current) clearInterval(pollRef.current);
    pollRef.current = setInterval(async () => {
      try {
        const data = await api.getUserSubmissions(username);
        setUserSubmissions(data ?? []);
      } catch {}
    }, 4000);
    return () => { if (pollRef.current) clearInterval(pollRef.current); };
  }, [hasInProgress, username]);

  // Whenever user submissions update, refresh the filtered list for the active exercise if we know its ids
  useEffect(() => {
    if (!activeId) return;
    const ids = exerciseSubmissionIds[activeId];
    if (!ids) return;
    const filtered = (userSubmissions || []).filter(s => ids.has(s.id));
    setSubmissionsByExercise(prev => ({ ...prev, [activeId]: filtered }));
  }, [userSubmissions, activeId, exerciseSubmissionIds]);

  // Compute and manage the lifecycle of the result image URL for the details modal
  useEffect(() => {
  const hasData = detailsModal?.open && detailsModal?.data;
  const newUrl = hasData ? resultImageDataUrl(detailsModal.data) : null;
  const newDataUrl = hasData ? resultImageDataUri(detailsModal.data) : null;
  setDetailsImageUrl(newUrl || null);
  setDetailsDataUrl(newDataUrl || null);
  // Reset src type to blob when data changes
  setDetailsImageSrcType('blob');
  // Reset measured heights on data changes
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
  }, [detailsModal.open, detailsModal.data]);

  function b64(str) {
    // handle unicode
    return btoa(unescape(encodeURIComponent(str)));
  }

  const onChangeCode = (exerciseId, value) => {
    setCodeByExercise(prev => ({ ...prev, [exerciseId]: value }));
  };

  const onSubmitCode = async (exerciseId) => {
    setError(null);
    setSubmitting(true);
    try {
      const code = codeByExercise[exerciseId] || '';
      if (!code.trim()) throw new Error('Please paste or type your code first');
      const sourceCodeBase64 = b64(code);
      await api.submitSolution(exerciseId, sourceCodeBase64);
      // Refresh user submissions and the exercise ids to include the new one
      const [user, ex] = await Promise.all([
        api.getUserSubmissions(username),
        api.getExercise(exerciseId),
      ]);
      setUserSubmissions(user ?? []);
      const refs = Array.isArray(ex?.submissions) ? ex.submissions : [];
      const ids = new Set(refs.map(r => r?.id).filter((x) => x != null));
      setExerciseSubmissionIds(prev => ({ ...prev, [exerciseId]: ids }));
      const refMap = {}; refs.forEach(r => { if (r && r.id != null) refMap[r.id] = r; });
      setExerciseSubmissionRefs(prev => ({ ...prev, [exerciseId]: refMap }));
      const filtered = (user ?? []).filter(s => ids.has(s.id));
      setSubmissionsByExercise(prev => ({ ...prev, [exerciseId]: filtered }));
    } catch (e) {
      setError(e.message || 'Submission failed');
    } finally {
      setSubmitting(false);
    }
  };

  const refreshExerciseSubmissions = async (exerciseId) => {
    setError(null);
    setRefreshing(prev => ({ ...prev, [exerciseId]: true }));
    try {
      const [user, ex] = await Promise.all([
        api.getUserSubmissions(username),
        api.getExercise(exerciseId),
      ]);
      setUserSubmissions(user ?? []);
      const refs = Array.isArray(ex?.submissions) ? ex.submissions : [];
      const ids = new Set(refs.map(r => r?.id).filter((x) => x != null));
      setExerciseSubmissionIds(prev => ({ ...prev, [exerciseId]: ids }));
      const refMap = {}; refs.forEach(r => { if (r && r.id != null) refMap[r.id] = r; });
      setExerciseSubmissionRefs(prev => ({ ...prev, [exerciseId]: refMap }));
      const filtered = (user ?? []).filter(s => ids.has(s.id));
      setSubmissionsByExercise(prev => ({ ...prev, [exerciseId]: filtered }));
    } catch (e) {
      setError(e.message || 'Failed to refresh submissions');
    } finally {
      setRefreshing(prev => ({ ...prev, [exerciseId]: false }));
    }
  };

  async function openSubmissionDetails(exId, submission) {
    const numericId = submission?.id;
    const ref = exerciseSubmissionRefs?.[exId]?.[numericId];
    let subIdStr = null;
    if (ref?.uri) {
      try {
        const raw = ref.uri.trim();
        const path = raw.includes('://') ? new URL(raw).pathname : raw;
        subIdStr = path.split('?')[0].split('#')[0].split('/').filter(Boolean).pop();
      } catch {}
    }
    if (!subIdStr && (submission?.submissionId || submission?.SubmissionId)) subIdStr = String(submission.submissionId || submission.SubmissionId);
    if (!subIdStr && numericId != null) subIdStr = String(numericId);
    setDetailsModal({ open: true, loading: true, data: null, error: null, id: subIdStr || numericId || '' });
    try {
      const details = await api.getSubmissionStatus(subIdStr || numericId);
      setDetailsModal({ open: true, loading: false, data: details, error: null, id: subIdStr || numericId || '' });
    } catch (e) {
      setDetailsModal({ open: true, loading: false, data: null, error: e?.message || 'Failed to load submission', id: subIdStr || numericId || '' });
    }
  }

  return (
    <div>
      <h2 className="h4 mb-3">My Exercises</h2>
      {loading ? (
        <div className="text-muted">Loading…</div>
      ) : exercises.length === 0 ? (
        <div className="alert alert-info">No exercises assigned yet.</div>
      ) : (
        <div>
          <ul className="nav nav-tabs" role="tablist">
            {exercises.map(ex => (
              <li key={ex.id} className="nav-item" role="presentation">
                <button
                  className={`nav-link ${activeId === ex.id ? 'active' : ''}`}
                  role="tab"
                  onClick={() => setActiveId(ex.id)}
                >
                  <div className="d-flex align-items-center gap-2">
                    {ex.imageBlobUrl && <img src={ex.imageBlobUrl} alt="thumb" className="gg-thumb-sm" />}
                    <div className="text-start">
                      <div className="fw-semibold small mb-0">{ex.title}</div>
                      {ex.description && (
                        <div className="text-muted small" title={ex.description}>
                          {truncateText(ex.description, 60)}
                        </div>
                      )}
                    </div>
                  </div>
                </button>
              </li>
            ))}
          </ul>

          <div className="tab-content border-start border-end border-bottom p-3">
            {exercises.map(ex => (
              <div
                key={ex.id}
                className={`tab-pane fade ${activeId === ex.id ? 'show active' : ''}`}
                role="tabpanel"
              >
                {/* Image centered, then title + full description under it */}
                <div className="text-center mb-3">
                  {ex.imageBlobUrl && <img src={ex.imageBlobUrl} alt="exercise" className="gg-image-full" />}
                </div>
                <h3 className="h5">{ex.title}</h3>
                {ex.description && <p className="mb-3">{ex.description}</p>}

                {/* Code editor with line numbers and C/C++ syntax */}
                <label className="form-label">Your C source code</label>
                <div className="border rounded overflow-hidden">
                  <Suspense fallback={<div className="p-3 text-muted">Loading editor…</div>}>
                    <CodeMirrorLazy
                      value={codeByExercise[ex.id] || ''}
                      height="260px"
                      extensions={editorExtensions}
                      onChange={(value) => onChangeCode(ex.id, value)}
                      placeholder="Paste your C source code here"
                    />
                  </Suspense>
                </div>

                <div className="d-flex gap-2 mt-2">
                  <button
                    className="btn btn-primary"
                    disabled={submitting}
                    onClick={() => onSubmitCode(ex.id)}
                  >
                    {submitting ? 'Submitting…' : 'Submit solution'}
                  </button>
                  <button
                    className="btn btn-outline-secondary"
                    disabled={submitting}
                    onClick={() => onChangeCode(ex.id, '')}
                  >
                    Clear
                  </button>
                </div>

                {/* Submissions for this exercise */}
                <div className="d-flex align-items-center justify-content-between mt-4">
                  <h4 className="h6 mb-0">Your Submissions</h4>
                  <button
                    type="button"
                    className="btn btn-outline-secondary btn-sm"
                    onClick={() => refreshExerciseSubmissions(ex.id)}
                    disabled={!!refreshing[ex.id]}
                    title="Refresh submissions"
                  >
                    {refreshing[ex.id] ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                        Refreshing
                      </>
                    ) : (
                      'Refresh'
                    )}
                  </button>
                </div>
                {error && activeId === ex.id && <div className="alert alert-danger py-2 my-2">{error}</div>}
                {(submissionsByExercise[ex.id]?.length ?? 0) > 0 ? (
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
                        {submissionsByExercise[ex.id].map((s) => (
                          <tr
                            key={s.id}
                            className="gg-submission-row"
                            onClick={() => openSubmissionDetails(ex.id, s)}
                            role="button"
                            tabIndex={0}
                            onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); openSubmissionDetails(ex.id, s); } }}
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
                  <div className="text-muted">No submissions yet.</div>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
  {renderDetailsModal(
    detailsModal,
    () => setDetailsModal({ open: false, loading: false, data: null, error: null, id: null }),
    detailsImageUrl,
    detailsDataUrl,
    detailsImageSrcType,
    setDetailsImageSrcType,
    detailsImgBg,
  setDetailsImgBg,
  editorExtensions,
  detailsLeftBoxH,
  setDetailsLeftBoxH
  )}
    </div>
  );
}

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
  // YIQ/contrast
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

// no-op placeholders removed; using user submissions intersection with exercise submission IDs

function truncateText(text, max = 60) {
  if (!text) return '';
  if (text.length <= max) return text;
  return text.slice(0, max - 3) + '...';
}

function renderDetailsModal(modal, onClose, imageUrl, dataUrl, srcType, setSrcType, imgBg, setImgBg, editorExtensions, detailsLeftBoxH, setDetailsLeftBoxH) {
  if (!modal?.open) return null;
  const d = modal.data;
  const chosenUrl = srcType === 'data' ? (dataUrl || imageUrl) : (imageUrl || dataUrl);
  const img = chosenUrl || (d ? resultImageDataUrl(d) : null);
  // Diagnostics removed per request
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
                                // Switch to data URL fallback if blob fails
                                if (setSrcType && srcType === 'blob') {
                                  setSrcType('data');
                                  e.currentTarget.style.display = 'none';
                                  e.currentTarget.parentElement.insertAdjacentHTML('beforeend', '<div class=\"text-muted\">Retrying with data URI…</div>');
                                } else {
                                  e.currentTarget.style.display = 'none';
                                  e.currentTarget.parentElement.insertAdjacentHTML('beforeend', '<div class=\"text-muted\">Unable to display result image.</div>');
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
                          <Suspense fallback={<div className="p-3 text-muted">Loading viewer…</div>}>
                            <CodeMirrorLazy
                              value={sourceCode}
                              height={`${editorHeightPx}px`}
                              extensions={editorExtensions}
                              editable={false}
                            />
                          </Suspense>
                        ) : (
                          <div className="p-3 text-muted">No source code available.</div>
                        )}
                      </div>
                    </div>
                  </div>
                  {/* diagnostics removed */}
                </div>
              )}
            </div>
            {/* footer close button removed; using header X */}
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

  // If it's already a data URL string
  if (typeof payload === 'string') {
    const trimmed = payload.trim();
    if (trimmed.startsWith('data:')) return trimmed;
    // Clean common artifacts (quotes, whitespace)
    const clean = trimmed.replace(/^\"|\"$/g, '').replace(/\s+/g, '');
    const type = mimeType || 'image/png';
    // Prefer object URL from decoded bytes for robustness
    try {
      const bytes = base64ToUint8(clean);
      if (bytes && bytes.length > 0) {
        const blob = new Blob([bytes], { type });
        return URL.createObjectURL(blob);
      }
    } catch {}
    // Fallback to data URI
    return `data:${type};base64,${clean}`;
  }

  // Buffer-like: array of bytes or Node buffer JSON
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

// Always returns a data: URI (base64) if possible, without constructing a Blob
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
  // If bytes array, make a minimal data URI by base64 encoding bytes
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
  // Handle URL-safe variants
  const norm = b64.replace(/-/g, '+').replace(/_/g, '/');
  // Pad to multiple of 4
  const padded = norm + '==='.slice((norm.length + 3) % 4);
  const bin = atob(padded);
  const len = bin.length;
  const bytes = new Uint8Array(len);
  for (let i = 0; i < len; i++) bytes[i] = bin.charCodeAt(i);
  return bytes;
}

// Decode base64 into a UTF-8 string (safe for unicode)
function base64ToUtf8(b64) {
  if (!b64) return '';
  try {
    const norm = b64.replace(/-/g, '+').replace(/_/g, '/');
    const padded = norm + '==='.slice((norm.length + 3) % 4);
    const bin = atob(padded);
    try {
      // decodeURIComponent(escape()) reverses btoa(unescape(encodeURIComponent()))
      return decodeURIComponent(escape(bin));
    } catch {
      // Fallback to binary-ascii if unicode decode fails
      return bin;
    }
  } catch {
    return '';
  }
}

function openSubmissionDetails(currentExerciseId, submission) {
  // this function will be replaced inline by closure in component, placeholder
}

// image diagnostics removed

// ---- Error/Result code helpers ----
function getSubmissionResultCode(details) {
  if (!details) return null;
  // Try common locations
  const sr = details.submissionResult || {};
  const candidates = [
    sr.errorCode,
    sr.resultCode,
    details.errorCode,
    details.resultCode,
  ];
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
