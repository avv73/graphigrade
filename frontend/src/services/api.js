// API layer: real endpoints only, per backend OpenAPI. No mocks.

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

function getToken() {
  return localStorage.getItem('gg_token');
}

function authHeaders() {
  const t = getToken();
  return t ? { Authorization: `Bearer ${t}` } : {};
}

async function parseResponse(res) {
  let data = null;
  const text = await res.text();
  if (text) {
    try { data = JSON.parse(text); } catch { data = text; }
  }
  if (!res.ok) {
    if (data.errors && data.errors["Password"]) { // dirty fix
       throw new Error(data.errors["Password"])
    }
    const message = (data && data.errorMessage) || data?.message || res.statusText || 'Request failed';
    throw new Error(message);
  }
  return data;
}

async function httpPost(path, body, options = {}) {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...authHeaders(),
      ...(options.headers || {}),
    },
    body: JSON.stringify(body),
  });
  return parseResponse(res);
}

async function httpGet(path, options = {}) {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'GET',
    headers: {
      ...authHeaders(),
      ...(options.headers || {}),
    },
  });
  return parseResponse(res);
}

async function httpPut(path, options = {}) {
  const res = await fetch(`${API_BASE}${path}`, {
    method: 'PUT',
    headers: {
      ...authHeaders(),
      ...(options.headers || {}),
    },
  });
  return parseResponse(res);
}

// Auth: real backend via OpenAPI
// POST /api/Auth/login -> { username, jwtToken }
export async function login(username, password) {
  const res = await httpPost('/api/Auth/login', { username, password }, { headers: {} });
  const token = res?.jwtToken;
  if (!token) throw new Error('No token returned from server');
  return { token, username: res?.username || username };
}

// POST /api/Auth/register -> 201 { username, jwtToken? }
export async function register({ username, password }) {
  const res = await httpPost('/api/Auth/register', { username, password }, { headers: {} });
  return { username: res?.username ?? username, token: res?.jwtToken ?? null };
}

// GET /api/User/{username}
export async function getUser(username) {
  if (!username) throw new Error('Username is required');
  return httpGet(`/api/User/${encodeURIComponent(username)}`);
}

// GET /api/Exercise/{id}
export async function getExercise(id) {
  if (id == null) throw new Error('Exercise id is required');
  return httpGet(`/api/Exercise/${encodeURIComponent(id)}`);
}

// POST /api/Exercise
// body: { title, description?, isVisible, groupIds: number[], expectedImageBase64 }
export async function createExercise({ title, description = null, isVisible = true, groupIds = [], expectedImageBase64 }) {
  if (!title) throw new Error('Title is required');
  if (!expectedImageBase64) throw new Error('Expected image (base64) is required');
  return httpPost('/api/Exercise', {
    title,
    description,
    isVisible,
    groupIds,
    expectedImageBase64,
  });
}

// POST /api/Exercise/{id}/submit
export async function submitSolution(exerciseId, sourceCodeBase64) {
  if (!exerciseId) throw new Error('Exercise id is required');
  if (!sourceCodeBase64) throw new Error('Source code (base64) is required');
  return httpPost(`/api/Exercise/${encodeURIComponent(exerciseId)}/submit`, {
    sourceCodeBase64,
  });
}

// GET /api/Submission/{submissionId}
export async function getSubmissionStatus(submissionId) {
  if (!submissionId) throw new Error('Submission id is required');
  return httpGet(`/api/Submission/${encodeURIComponent(submissionId)}`);
}

// GET /api/Submission/user/{username}
export async function getUserSubmissions(username) {
  if (!username) throw new Error('Username is required');
  const res = await httpGet(`/api/Submission/user/${encodeURIComponent(username)}`);
  return res?.submissions || [];
}

// POST /api/Group
export async function createGroup({ groupName, userIds = [] }) {
  if (!groupName) throw new Error('Group name is required');
  return httpPost('/api/Group', { groupName, userIds });
}

// GET /api/Group/{id}
export async function getGroup(id) {
  if (id == null) throw new Error('Group id is required');
  return httpGet(`/api/Group/${encodeURIComponent(id)}`);
}

// GET /api/Group/all
export async function getAllGroups() {
  const res = await httpGet('/api/Group/all');
  return res?.groups || [];
}

// GET /api/User/students
export async function getAllStudents() {
  const res = await httpGet('/api/User/students');
  return res?.users || [];
}

// PUT /api/Exercise/{id}/assign/{group_id}
export async function assignExerciseToGroup(exerciseId, groupId) {
  if (!exerciseId || !groupId) throw new Error('Exercise and Group ids are required');
  return httpPut(`/api/Exercise/${encodeURIComponent(exerciseId)}/assign/${encodeURIComponent(groupId)}`);
}

// PUT /api/Group/{id}/assign/{student_id}
export async function assignUserToGroup(groupId, studentId) {
  if (!groupId || !studentId) throw new Error('Group id and Student id are required');
  return httpPut(`/api/Group/${encodeURIComponent(groupId)}/assign/${encodeURIComponent(studentId)}`);
}

// Convenience helpers used by pages
export async function fetchStudentExercises(username) {
  const user = await getUser(username);
  const items = user?.availableExercises || [];
  // Fetch details for each exercise id to get title/description
  const detailed = await Promise.all(items.map(async (r) => {
    try {
      const id = r?.id;
      const ex = await getExercise(id);
      return { id, title: ex?.title || r?.name || `Exercise ${id}`, description: ex?.description || '', imageBlobUrl: ex?.imageBlobUrl || null };
    } catch {
      return { id: r?.id, title: r?.name || `Exercise ${r?.id}`, description: '', imageBlobUrl: null };
    }
  }));
  return detailed;
}

export async function fetchAllExercisesForUser(username) {
  return fetchStudentExercises(username);
}
