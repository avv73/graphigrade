import { useEffect, useState } from 'react';
import * as api from '../services/api.js';
import { useAuth } from '../contexts/AuthContext.jsx';

export default function StudentDashboard() {
  const { username } = useAuth();
  const [exercises, setExercises] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    api.fetchStudentExercises(username)
      .then((data) => { if (isMounted) setExercises(data); })
      .catch(() => { if (isMounted) setExercises([]); })
      .finally(() => setLoading(false));
    return () => { isMounted = false; };
  }, [username]);

  return (
    <div>
      <h2 className="h4 mb-3">My Exercises</h2>
      {loading ? (
        <div className="text-muted">Loading…</div>
      ) : exercises.length === 0 ? (
        <div className="alert alert-info">No exercises assigned yet.</div>
      ) : (
        <div className="list-group">
          {exercises.map(ex => (
            <div key={ex.id} className="list-group-item">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <div className="fw-semibold">{ex.title}</div>
                  {ex.description && <div className="text-muted small">{ex.description}</div>}
                </div>
                {ex.imageBlobUrl && (
                  <img src={ex.imageBlobUrl} alt="expected" style={{ maxHeight: 40 }} />
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
