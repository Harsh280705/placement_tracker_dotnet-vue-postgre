// Centralized HTTP/JSON layer for the Placement Tracker API.
// Vue components must use these helpers instead of calling fetch directly.
// Relative /api path: works through the NGINX entry point (:8080) and through
// the Vite dev server (:5173, which proxies /api to the backend). Override
// with VITE_API_BASE_URL only if the API lives on a different origin.
const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api/applications';

async function handleResponse(response) {
  if (response.status === 204) {
    return null;
  }
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const message = data?.message || data?.title || `Request failed with status ${response.status}`;
    const error = new Error(message);
    error.status = response.status;
    error.details = data;
    throw error;
  }
  return data;
}

async function request(url, options = {}) {
  const response = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...options
  });
  return handleResponse(response);
}

export function getApplications() {
  return request(API_BASE);
}

export function getApplication(id) {
  return request(`${API_BASE}/${id}`);
}

export function createApplication(data) {
  return request(API_BASE, { method: 'POST', body: JSON.stringify(data) });
}

export function updateApplication(id, data) {
  return request(`${API_BASE}/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}

export async function deleteApplication(id) {
  const response = await fetch(`${API_BASE}/${id}`, { method: 'DELETE' });
  return handleResponse(response);
}

export const allowedStatuses = ['Wishlist', 'Applied', 'Interview', 'Offer', 'Rejected'];
