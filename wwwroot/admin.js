const loginView = document.getElementById('login-view');
const dashboardView = document.getElementById('dashboard-view');
const loginForm = document.getElementById('login-form');
const emailInput = document.getElementById('email');
const passwordInput = document.getElementById('password');
const logoutBtn = document.getElementById('logout-btn');
const statusBox = document.getElementById('status-box');
const submitBtn = document.getElementById('submit-btn');
const togglePasswordBtn = document.getElementById('toggle-password');
const togglePasswordIcon = document.getElementById('toggle-password-icon');

// Project Modal
const projectModal = document.getElementById('project-modal');
const projectForm = document.getElementById('project-form');
const modalTitle = document.getElementById('modal-title');
const modalStatus = document.getElementById('modal-status');
const projectIdInput = document.getElementById('project-id');
const projectTitleInput = document.getElementById('project-title');
const projectDescInput = document.getElementById('project-description');
const projectGithubInput = document.getElementById('project-github-url');
const projectLiveInput = document.getElementById('project-live-url');
const projectImageInput = document.getElementById('project-image-url');
const addProjectBtn = document.getElementById('add-project-btn');
const modalCloseBtn = document.getElementById('modal-close-btn');
const modalCancelBtn = document.getElementById('modal-cancel-btn');
const modalSaveBtn = document.getElementById('modal-save-btn');
const projectsTableBody = document.getElementById('projects-table-body');

// Skills
const tabProjects = document.getElementById('tab-projects');
const tabSkills = document.getElementById('tab-skills');
const panelProjects = document.getElementById('panel-projects');
const panelSkills = document.getElementById('panel-skills');

const skillModal = document.getElementById('skill-modal');
const skillForm = document.getElementById('skill-form');
const skillModalTitle = document.getElementById('skill-modal-title');
const skillModalStatus = document.getElementById('skill-modal-status');
const skillIdInput = document.getElementById('skill-id');
const skillNameInput = document.getElementById('skill-name');
const skillCategoryInput = document.getElementById('skill-category');
const skillLevelInput = document.getElementById('skill-level');
const addSkillBtn = document.getElementById('add-skill-btn');
const skillModalCloseBtn = document.getElementById('skill-modal-close-btn');
const skillModalCancelBtn = document.getElementById('skill-modal-cancel-btn');
const skillModalSaveBtn = document.getElementById('skill-modal-save-btn');
const skillsTableBody = document.getElementById('skills-table-body');

// Social Links
const tabSocialLinks = document.getElementById('tab-social-links');
const panelSocialLinks = document.getElementById('panel-social-links');

const socialLinkModal = document.getElementById('social-link-modal');
const socialLinkForm = document.getElementById('social-link-form');
const socialLinkModalTitle = document.getElementById('social-link-modal-title');
const socialLinkModalStatus = document.getElementById('social-link-modal-status');
const socialLinkIdInput = document.getElementById('social-link-id');
const socialLinkPlatformInput = document.getElementById('social-link-platform');
const socialLinkUrlInput = document.getElementById('social-link-url');
const addSocialLinkBtn = document.getElementById('add-social-link-btn');
const socialLinkModalCloseBtn = document.getElementById('social-link-modal-close-btn');
const socialLinkModalCancelBtn = document.getElementById('social-link-modal-cancel-btn');
const socialLinkModalSaveBtn = document.getElementById('social-link-modal-save-btn');
const socialLinksTableBody = document.getElementById('social-links-table-body');

// Profile
const tabProfile = document.getElementById('tab-profile');
const panelProfile = document.getElementById('panel-profile');
const profileCard = document.getElementById('profile-card');
const profileEmptyState = document.getElementById('profile-empty-state');
const editProfileBtn = document.getElementById('edit-profile-btn');

const profileViewFullname = document.getElementById('profile-view-fullname');
const profileViewTitle = document.getElementById('profile-view-title');
const profileViewEmail = document.getElementById('profile-view-email');
const profileViewLocation = document.getElementById('profile-view-location');
const profileViewImage = document.getElementById('profile-view-image');
const profileViewBio = document.getElementById('profile-view-bio');

const profileModal = document.getElementById('profile-modal');
const profileForm = document.getElementById('profile-form');
const profileModalTitle = document.getElementById('profile-modal-title');
const profileModalStatus = document.getElementById('profile-modal-status');
const profileIdInput = document.getElementById('profile-id');
const profileFullnameInput = document.getElementById('profile-fullname');
const profileTitleInput = document.getElementById('profile-title');
const profileEmailInput = document.getElementById('profile-email');
const profileLocationInput = document.getElementById('profile-location');
const profileImageUrlInput = document.getElementById('profile-image-url');
const profileBioInput = document.getElementById('profile-bio');
const profileModalCloseBtn = document.getElementById('profile-modal-close-btn');
const profileModalCancelBtn = document.getElementById('profile-modal-cancel-btn');
const profileModalSaveBtn = document.getElementById('profile-modal-save-btn');

let projectsCache = [];
let skillsCache = [];
let socialLinksCache = [];
let profileCache = null;
let projectSortable = null;
let skillSortable = null;
let socialLinkSortable = null;
let mouseDownOnBackdrop = false;
let mouseDownOnSkillBackdrop = false;
let mouseDownOnSocialLinkBackdrop = false;
let mouseDownOnProfileBackdrop = false;
let mouseDownOnLogoutBackdrop = false;
let isRefreshing = false;
let refreshPromise = null;

// ---------- Status Helpers ----------
function showStatus(message, isError = true) {
  statusBox.textContent = message;
  statusBox.className = isError
    ? 'w-full max-w-5xl mb-4 p-3 rounded-lg text-sm font-medium bg-rose-950/80 border border-rose-500/50 text-rose-200'
    : 'w-full max-w-5xl mb-4 p-3 rounded-lg text-sm font-medium bg-teal-950/80 border border-teal-500/50 text-teal-200';
  statusBox.classList.remove('hidden');
}

function clearStatus() {
  statusBox.textContent = '';
  statusBox.classList.add('hidden');
}

function showModalStatus(message, isError = true) {
  modalStatus.textContent = message;
  modalStatus.className = isError
    ? 'mb-3 p-3 rounded-lg text-sm font-medium bg-rose-950/80 border border-rose-500/50 text-rose-200'
    : 'mb-3 p-3 rounded-lg text-sm font-medium bg-teal-950/80 border border-teal-500/50 text-teal-200';
  modalStatus.classList.remove('hidden');
}

function clearModalStatus() {
  modalStatus.textContent = '';
  modalStatus.classList.add('hidden');
}

function showSkillModalStatus(message, isError = true) {
  if (!skillModalStatus) return;
  skillModalStatus.textContent = message;
  skillModalStatus.className = isError
    ? 'mb-3 p-3 rounded-lg text-sm font-medium bg-rose-950/80 border border-rose-500/50 text-rose-200'
    : 'mb-3 p-3 rounded-lg text-sm font-medium bg-teal-950/80 border border-teal-500/50 text-teal-200';
  skillModalStatus.classList.remove('hidden');
}

function clearSkillModalStatus() {
  if (!skillModalStatus) return;
  skillModalStatus.textContent = '';
  skillModalStatus.classList.add('hidden');
}

function showSocialLinkModalStatus(message, isError = true) {
  if (!socialLinkModalStatus) return;
  socialLinkModalStatus.textContent = message;
  socialLinkModalStatus.className = isError
    ? 'mb-3 p-3 rounded-lg text-sm font-medium bg-rose-950/80 border border-rose-500/50 text-rose-200'
    : 'mb-3 p-3 rounded-lg text-sm font-medium bg-teal-950/80 border border-teal-500/50 text-teal-200';
  socialLinkModalStatus.classList.remove('hidden');
}

function clearSocialLinkModalStatus() {
  if (!socialLinkModalStatus) return;
  socialLinkModalStatus.textContent = '';
  socialLinkModalStatus.classList.add('hidden');
}

function showProfileModalStatus(message, isError = true) {
  if (!profileModalStatus) return;
  profileModalStatus.textContent = message;
  profileModalStatus.className = isError
    ? 'mb-3 p-3 rounded-lg text-sm font-medium bg-rose-950/80 border border-rose-500/50 text-rose-200'
    : 'mb-3 p-3 rounded-lg text-sm font-medium bg-teal-950/80 border border-teal-500/50 text-teal-200';
  profileModalStatus.classList.remove('hidden');
}

function clearProfileModalStatus() {
  if (!profileModalStatus) return;
  profileModalStatus.textContent = '';
  profileModalStatus.classList.add('hidden');
}

function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

function extractErrorMessage(data, fallback) {
  if (!data) return fallback;
  if (typeof data === 'string') return data;
  if (data.detail) return data.detail;
  if (data.message) return data.message;
  if (data.title && data.title !== 'One or more validation errors occurred.') return data.title;
  if (data.errors && typeof data.errors === 'object') {
    const messages = [];
    for (const key in data.errors) {
      if (Array.isArray(data.errors[key])) messages.push(...data.errors[key]);
    }
    if (messages.length > 0) return messages.join(' | ');
  }
  return fallback;
}

// ---------- Logout Modal ----------
function injectLogoutModal() {
  if (document.getElementById('logout-confirm-modal')) return;
  const html = `
    <div id="logout-confirm-modal" class="hidden fixed inset-0 z-[60] flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm">
      <div class="w-full max-w-sm bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-2xl">
        <div class="flex flex-col items-center text-center">
          <div class="w-14 h-14 rounded-full bg-rose-500/10 border border-rose-500/20 text-rose-400 flex items-center justify-center mb-4 text-2xl">
            <i class="fa-solid fa-right-from-bracket"></i>
          </div>
          <h3 class="text-lg font-bold text-white mb-1.5">Sign out?</h3>
          <p class="text-sm text-slate-400 leading-relaxed mb-6">You'll need to sign in again to access the admin dashboard.</p>
          <div class="flex gap-3 w-full">
            <button type="button" id="logout-cancel-btn" class="flex-1 py-2.5 px-4 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold text-sm transition-colors cursor-pointer">Cancel</button>
            <button type="button" id="logout-confirm-btn" class="flex-1 py-2.5 px-4 rounded-lg bg-rose-500 hover:bg-rose-400 text-white font-semibold text-sm transition-colors cursor-pointer">Sign Out</button>
          </div>
        </div>
      </div>
    </div>`;
  document.body.insertAdjacentHTML('beforeend', html);

  const logoutModal = document.getElementById('logout-confirm-modal');
  document.getElementById('logout-cancel-btn').addEventListener('click', hideLogoutConfirm);
  document.getElementById('logout-confirm-btn').addEventListener('click', () => { hideLogoutConfirm(); performLogout(); });

  logoutModal.addEventListener('mousedown', (e) => { mouseDownOnLogoutBackdrop = e.target === logoutModal; });
  logoutModal.addEventListener('mouseup', (e) => {
    if (mouseDownOnLogoutBackdrop && e.target === logoutModal) hideLogoutConfirm();
    mouseDownOnLogoutBackdrop = false;
  });
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && !logoutModal.classList.contains('hidden')) hideLogoutConfirm();
  });
}

function showLogoutConfirm() {
  const m = document.getElementById('logout-confirm-modal');
  if (m) m.classList.remove('hidden');
}
function hideLogoutConfirm() {
  const m = document.getElementById('logout-confirm-modal');
  if (m) m.classList.add('hidden');
}
function performLogout() {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  clearStatus();
  showLogin();
}

// ---------- Auth Refresh ----------
async function refreshAccessToken() {
  if (isRefreshing && refreshPromise) return refreshPromise;
  isRefreshing = true;
  refreshPromise = (async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return null;
    try {
      const res = await fetch('/api/Auth/Refresh', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
      });
      if (!res.ok) return null;
      const data = await res.json().catch(() => null);
      const newAccess = data?.accessToken || data?.AccessToken;
      const newRefresh = data?.refreshToken || data?.RefreshToken;
      if (!newAccess) return null;
      localStorage.setItem('accessToken', newAccess);
      if (newRefresh) localStorage.setItem('refreshToken', newRefresh);
      return newAccess;
    } catch { return null; }
    finally { isRefreshing = false; refreshPromise = null; }
  })();
  return refreshPromise;
}

function forceLogout(reason) {
  localStorage.removeItem('accessToken');
  localStorage.removeItem('refreshToken');
  clearStatus();
  clearModalStatus();
  clearSkillModalStatus();
  clearSocialLinkModalStatus();
  clearProfileModalStatus();
  closeProjectModal();
  closeSkillModal();
  closeSocialLinkModal();
  closeProfileModal();
  hideLogoutConfirm();
  showStatus(reason || 'Your session expired. Please sign in again.', true);
  showLogin();
}

async function fetchWithAuth(url, options = {}) {
  const doFetch = (token) => fetch(url, {
    ...options,
    headers: { ...(options.headers || {}), ...(token ? { Authorization: `Bearer ${token}` } : {}) }
  });
  let res = await doFetch(localStorage.getItem('accessToken'));
  if (res.status === 401) {
    const newToken = await refreshAccessToken();
    if (newToken) res = await doFetch(newToken);
    else { forceLogout('Your session expired. Please sign in again.'); throw new Error('SESSION_EXPIRED'); }
  }
  return res;
}

// ---------- Projects ----------
function openProjectModal(project = null) {
  projectForm.reset();
  clearModalStatus();
  if (project) {
    const id = project.projectId ?? project.id ?? project.ProjectId ?? '';
    modalTitle.textContent = 'Edit Project';
    projectIdInput.value = id;
    projectTitleInput.value = project.title || project.Title || '';
    projectDescInput.value = project.description || project.Description || '';
    projectGithubInput.value = project.gitHubUrl || project.githubUrl || project.GitHubUrl || '';
    projectLiveInput.value = project.liveUrl || project.LiveUrl || '';
    projectImageInput.value = project.imageUrl || project.ImageUrl || '';
  } else {
    modalTitle.textContent = 'Add Project';
    projectIdInput.value = '';
  }
  projectModal.classList.remove('hidden');
}

function closeProjectModal() {
  projectModal.classList.add('hidden');
  projectForm.reset();
  clearModalStatus();
}

async function fetchProjects() {
  projectsTableBody.innerHTML = `<tr><td colspan="4" class="py-6 px-4 text-center text-slate-500">Loading projects...</td></tr>`;
  try {
    const res = await fetchWithAuth('/api/Project/Get/All');
    if (!res.ok) {
      showStatus('Failed to load projects.', true);
      projectsTableBody.innerHTML = `<tr><td colspan="4" class="py-6 px-4 text-center text-rose-400">Error loading projects.</td></tr>`;
      return;
    }
    const data = await res.json();
    let projects = [];
    if (Array.isArray(data)) projects = data;
    else if (Array.isArray(data?.projects)) projects = data.projects;
    else if (Array.isArray(data?.data)) projects = data.data;
    else if (Array.isArray(data?.items)) projects = data.items;

    projects.sort((a, b) => {
      const aOrder = Number(a.displayOrder ?? a.DisplayOrder ?? 0);
      const bOrder = Number(b.displayOrder ?? b.DisplayOrder ?? 0);
      if (aOrder !== bOrder) return aOrder - bOrder;
      const aId = Number(a.projectId ?? a.id ?? a.ProjectId ?? 0);
      const bId = Number(b.projectId ?? b.id ?? b.ProjectId ?? 0);
      return bId - aId;
    });

    projectsCache = projects;
    renderProjectsTable(projects);
    initProjectSortable();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Error connecting to project service.', true);
  }
}

function renderProjectsTable(projects) {
  if (!projects || projects.length === 0) {
    projectsTableBody.innerHTML = `<tr><td colspan="4" class="py-6 px-4 text-center text-slate-400">No projects found. Add one above!</td></tr>`;
    return;
  }
  projectsTableBody.innerHTML = projects.map(p => {
    const id = p.projectId ?? p.id ?? p.ProjectId;
    const title = p.title || p.Title || 'Untitled';
    const description = p.description || p.Description || '';
    const truncatedDesc = description.length > 75 ? `${description.slice(0, 75)}...` : description;
    return `
      <tr data-project-id="${id}" class="hover:bg-slate-900/60 transition-colors">
        <td class="py-3 px-2 w-10 text-center">
          <i class="fa-solid fa-grip-vertical text-slate-500 hover:text-teal-400 cursor-grab drag-handle transition-colors"></i>
        </td>
        <td class="py-3 px-4 font-medium text-white max-w-[200px] truncate" title="${escapeHtml(title)}">${escapeHtml(title)}</td>
        <td class="py-3 px-4 text-slate-400 max-w-[320px] truncate" title="${escapeHtml(description)}">${escapeHtml(truncatedDesc)}</td>
        <td class="py-3 px-4 text-right space-x-2 whitespace-nowrap">
          <button type="button" onclick="editProject('${id}')" class="py-1 px-2.5 rounded bg-slate-800 hover:bg-slate-700 text-teal-400 text-xs font-medium transition-colors cursor-pointer"><i class="fa-solid fa-pen-to-square mr-1"></i>Edit</button>
          <button type="button" onclick="deleteProject('${id}')" class="py-1 px-2.5 rounded bg-slate-800 hover:bg-rose-950/60 text-rose-400 text-xs font-medium transition-colors cursor-pointer"><i class="fa-solid fa-trash mr-1"></i>Delete</button>
        </td>
      </tr>`;
  }).join('');
}

function initProjectSortable() {
  if (projectSortable) projectSortable.destroy();
  projectSortable = new Sortable(projectsTableBody, {
    animation: 150,
    handle: '.drag-handle',
    ghostClass: 'sortable-ghost',
    dragClass: 'sortable-drag',
    onEnd: handleProjectReorder
  });
}

async function handleProjectReorder() {
  const rows = Array.from(projectsTableBody.querySelectorAll('tr[data-project-id]'));

  const items = rows.map((r, i) => ({
    projectId: parseInt(r.dataset.projectId, 10),
    displayOrder: i + 1
  }));

  if (items.length === 0) return;

  showStatus('Saving new order...', false);

  try {
    const res = await fetchWithAuth('/api/Project/Reorder', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(items)
    });

    if (!res.ok) {
      const data = await res.json().catch(() => null);
      const msg = extractErrorMessage(data, `Failed to save order (HTTP ${res.status}).`);
      showStatus(msg, true);
      await fetchProjects();
      return;
    }

    showStatus('Order updated successfully.', false);
    await fetchProjects();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Network error while saving order.', true);
    await fetchProjects();
  }
}

// ---------- Confirm Modal ----------
function showConfirmModal({ title, message, confirmText = 'Confirm', isDestructive = true }) {
  return new Promise((resolve) => {
    const existing = document.getElementById('custom-confirm-modal');
    if (existing) existing.remove();
    const overlay = document.createElement('div');
    overlay.id = 'custom-confirm-modal';
    overlay.className = 'fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm';
    overlay.innerHTML = `
      <div class="w-full max-w-sm bg-slate-900 border border-slate-800 rounded-2xl p-6 shadow-2xl text-center">
        <div class="w-12 h-12 rounded-full ${isDestructive ? 'bg-rose-500/10 border-rose-500/20 text-rose-400' : 'bg-teal-500/10 border-teal-500/20 text-teal-400'} border flex items-center justify-center mx-auto mb-4 text-xl">
          <i class="fa-solid ${isDestructive ? 'fa-triangle-exclamation' : 'fa-circle-question'}"></i>
        </div>
        <h3 class="text-lg font-bold text-white mb-2">${escapeHtml(title)}</h3>
        <p class="text-slate-400 text-sm mb-6">${escapeHtml(message)}</p>
        <div class="flex items-center justify-center gap-3">
          <button type="button" id="confirm-modal-cancel" class="flex-1 py-2 px-4 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold text-sm transition-colors cursor-pointer">Cancel</button>
          <button type="button" id="confirm-modal-confirm" class="flex-1 py-2 px-4 rounded-lg ${isDestructive ? 'bg-rose-600 hover:bg-rose-500 text-white' : 'bg-teal-500 hover:bg-teal-400 text-slate-950'} font-semibold text-sm transition-colors cursor-pointer">${escapeHtml(confirmText)}</button>
        </div>
      </div>`;
    document.body.appendChild(overlay);
    const cleanup = (result) => {
      document.removeEventListener('keydown', onKeyDown);
      overlay.remove();
      resolve(result);
    };
    const onKeyDown = (e) => { if (e.key === 'Escape') cleanup(false); };
    document.addEventListener('keydown', onKeyDown);
    let mdb = false;
    overlay.addEventListener('mousedown', (e) => { mdb = (e.target === overlay); });
    overlay.addEventListener('mouseup', (e) => { if (mdb && e.target === overlay) cleanup(false); mdb = false; });
    overlay.querySelector('#confirm-modal-cancel').addEventListener('click', () => cleanup(false));
    overlay.querySelector('#confirm-modal-confirm').addEventListener('click', () => cleanup(true));
  });
}

// ---------- Project Actions ----------
window.editProject = function(id) {
  const p = projectsCache.find(x => String(x.projectId ?? x.id ?? x.ProjectId) === String(id));
  if (p) openProjectModal(p);
  else showStatus('Project not found.', true);
};

window.deleteProject = async function(id) {
  const confirmed = await showConfirmModal({
    title: 'Delete Project',
    message: 'Are you sure you want to delete this project? This action cannot be undone.',
    confirmText: 'Delete', isDestructive: true
  });
  if (!confirmed) return;
  clearStatus();
  try {
    const res = await fetchWithAuth(`/api/Project/Delete/${id}`, { method: 'DELETE' });
    if (!res.ok) {
      const data = await res.json().catch(() => null);
      showStatus(extractErrorMessage(data, `Failed to delete project (HTTP ${res.status}).`), true);
      return;
    }
    showStatus('Project deleted successfully.', false);
    await fetchProjects();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Network error while deleting project.', true);
  }
};

projectForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  clearModalStatus();
  const id = projectIdInput.value.trim();
  const title = projectTitleInput.value.trim();
  const description = projectDescInput.value.trim();
  const gitHubUrl = projectGithubInput.value.trim() || null;
  const liveUrl = projectLiveInput.value.trim() || null;
  const imageUrl = projectImageInput.value.trim() || null;
  if (!title || !description) { showModalStatus('Title and description are required.', true); return; }
  const payload = { title, description, gitHubUrl, liveUrl, imageUrl };
  const isEdit = !!id;
  const url = isEdit ? `/api/Project/Update/${id}` : '/api/Project/Add';
  const method = isEdit ? 'PUT' : 'POST';
  const originalText = modalSaveBtn.textContent;
  modalSaveBtn.disabled = true;
  modalSaveBtn.classList.add('opacity-70', 'cursor-not-allowed');
  modalSaveBtn.textContent = isEdit ? 'Updating...' : 'Saving...';
  try {
    const res = await fetchWithAuth(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json().catch(() => null);
    if (!res.ok) {
      showModalStatus(extractErrorMessage(data, isEdit ? `Failed to update project (HTTP ${res.status}).` : `Failed to add project (HTTP ${res.status}).`), true);
      return;
    }
    closeProjectModal();
    showStatus(isEdit ? 'Project updated successfully.' : 'Project added successfully.', false);
    await fetchProjects();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showModalStatus(`Error: ${err.message || 'Network error.'}`, true);
  } finally {
    modalSaveBtn.disabled = false;
    modalSaveBtn.classList.remove('opacity-70', 'cursor-not-allowed');
    modalSaveBtn.textContent = originalText;
  }
});

addProjectBtn.addEventListener('click', () => openProjectModal());
modalCloseBtn.addEventListener('click', closeProjectModal);
modalCancelBtn.addEventListener('click', closeProjectModal);
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && !projectModal.classList.contains('hidden')) closeProjectModal();
});
projectModal.addEventListener('mousedown', (e) => { mouseDownOnBackdrop = e.target === projectModal; });
projectModal.addEventListener('mouseup', (e) => {
  if (mouseDownOnBackdrop && e.target === projectModal) closeProjectModal();
  mouseDownOnBackdrop = false;
});

// ---------- Tabs ----------
const ACTIVE_TAB_CLASS = 'px-4 py-2 rounded-lg bg-teal-500/10 text-teal-400 border border-teal-500/30 flex items-center gap-2 cursor-pointer';
const INACTIVE_TAB_CLASS = 'px-4 py-2 rounded-lg text-slate-400 hover:text-slate-300 transition-colors flex items-center gap-2 cursor-pointer';

function switchTab(tabName) {
  if (tabName === 'projects') {
    panelProjects.classList.remove('hidden');
    panelSkills.classList.add('hidden');
    panelSocialLinks.classList.add('hidden');
    panelProfile.classList.add('hidden');
    tabProjects.className = ACTIVE_TAB_CLASS;
    tabSkills.className = INACTIVE_TAB_CLASS;
    tabSocialLinks.className = INACTIVE_TAB_CLASS;
    tabProfile.className = INACTIVE_TAB_CLASS;
  } else if (tabName === 'skills') {
    panelProjects.classList.add('hidden');
    panelSkills.classList.remove('hidden');
    panelSocialLinks.classList.add('hidden');
    panelProfile.classList.add('hidden');
    tabSkills.className = ACTIVE_TAB_CLASS;
    tabProjects.className = INACTIVE_TAB_CLASS;
    tabSocialLinks.className = INACTIVE_TAB_CLASS;
    tabProfile.className = INACTIVE_TAB_CLASS;
    fetchSkills();
  } else if (tabName === 'social-links') {
    panelProjects.classList.add('hidden');
    panelSkills.classList.add('hidden');
    panelSocialLinks.classList.remove('hidden');
    panelProfile.classList.add('hidden');
    tabSocialLinks.className = ACTIVE_TAB_CLASS;
    tabProjects.className = INACTIVE_TAB_CLASS;
    tabSkills.className = INACTIVE_TAB_CLASS;
    tabProfile.className = INACTIVE_TAB_CLASS;
    fetchSocialLinks();
  } else if (tabName === 'profile') {
    panelProjects.classList.add('hidden');
    panelSkills.classList.add('hidden');
    panelSocialLinks.classList.add('hidden');
    panelProfile.classList.remove('hidden');
    tabProfile.className = ACTIVE_TAB_CLASS;
    tabProjects.className = INACTIVE_TAB_CLASS;
    tabSkills.className = INACTIVE_TAB_CLASS;
    tabSocialLinks.className = INACTIVE_TAB_CLASS;
    fetchProfile();
  }
}

// ---------- Skills ----------
function openSkillModal(skill = null) {
  skillForm.reset();
  clearSkillModalStatus();
  if (skill) {
    const id = skill.skillId ?? skill.id ?? skill.SkillId ?? '';
    skillModalTitle.textContent = 'Edit Skill';
    skillIdInput.value = id;
    skillNameInput.value = skill.skillName || skill.SkillName || skill.name || '';
    skillCategoryInput.value = skill.skillCategory || skill.SkillCategory || skill.category || '';
    skillLevelInput.value = String(skill.skillLevel ?? skill.SkillLevel ?? skill.level ?? 3);
  } else {
    skillModalTitle.textContent = 'Add Skill';
    skillIdInput.value = '';
    skillLevelInput.value = '3';
  }
  skillModal.classList.remove('hidden');
}

function closeSkillModal() {
  skillModal.classList.add('hidden');
  clearSkillModalStatus();
  skillForm.reset();
}

async function fetchSkills() {
  skillsTableBody.innerHTML = `<tr><td colspan="5" class="py-6 px-4 text-center text-slate-500">Loading skills...</td></tr>`;
  try {
    const res = await fetchWithAuth('/api/Skill/Get/All');
    if (!res.ok) {
      showStatus('Failed to load skills.', true);
      skillsTableBody.innerHTML = `<tr><td colspan="5" class="py-6 px-4 text-center text-rose-400">Error loading skills.</td></tr>`;
      return;
    }
    const data = await res.json();
    let skills = [];
    if (Array.isArray(data)) skills = data;
    else if (Array.isArray(data?.skills)) skills = data.skills;
    else if (Array.isArray(data?.data)) skills = data.data;
    else if (Array.isArray(data?.items)) skills = data.items;

    skills.sort((a, b) => {
      const aOrder = Number(a.displayOrder ?? a.DisplayOrder ?? 0);
      const bOrder = Number(b.displayOrder ?? b.DisplayOrder ?? 0);
      if (aOrder !== bOrder) return aOrder - bOrder;
      const aId = Number(a.skillId ?? a.id ?? a.SkillId ?? 0);
      const bId = Number(b.skillId ?? b.id ?? b.SkillId ?? 0);
      return bId - aId;
    });

    skillsCache = skills;
    renderSkillsTable(skills);
    initSkillSortable();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Error connecting to skill service.', true);
  }
}

function renderSkillsTable(skills) {
  if (!skills || skills.length === 0) {
    skillsTableBody.innerHTML = `<tr><td colspan="5" class="py-6 px-4 text-center text-slate-400">No skills found. Add one above!</td></tr>`;
    return;
  }
  skillsTableBody.innerHTML = skills.map(skill => {
    const id = skill.skillId ?? skill.id ?? skill.SkillId;
    const name = skill.skillName || skill.SkillName || skill.name || 'Unnamed';
    const category = skill.skillCategory || skill.SkillCategory || skill.category || 'General';
    const level = Math.max(1, Math.min(5, Number(skill.skillLevel ?? skill.SkillLevel ?? skill.level ?? 1)));
    const filledDots = '●'.repeat(level);
    const emptyDots = '○'.repeat(5 - level);

    return `
      <tr data-skill-id="${id}" class="hover:bg-slate-900/60 transition-colors">
        <td class="py-3 px-2 w-10 text-center">
          <i class="fa-solid fa-grip-vertical text-slate-500 hover:text-teal-400 cursor-grab drag-handle transition-colors"></i>
        </td>
        <td class="py-3 px-4 font-medium text-white max-w-[200px] truncate" title="${escapeHtml(name)}">${escapeHtml(name)}</td>
        <td class="py-3 px-4 text-slate-400 max-w-[180px] truncate" title="${escapeHtml(category)}">
          <span class="inline-block py-0.5 px-2 rounded-md bg-slate-800 border border-slate-700 text-xs text-slate-300">${escapeHtml(category)}</span>
        </td>
        <td class="py-3 px-4 text-teal-400 font-mono text-sm tracking-widest whitespace-nowrap" title="${level}/5">${filledDots}<span class="text-slate-600">${emptyDots}</span></td>
        <td class="py-3 px-4 text-right space-x-2 whitespace-nowrap">
          <button type="button" onclick="editSkill('${id}')" class="py-1 px-2.5 rounded bg-slate-800 hover:bg-slate-700 text-teal-400 text-xs font-medium transition-colors cursor-pointer"><i class="fa-solid fa-pen-to-square mr-1"></i>Edit</button>
          <button type="button" onclick="deleteSkill('${id}')" class="py-1 px-2.5 rounded bg-slate-800 hover:bg-rose-950/60 text-rose-400 text-xs font-medium transition-colors cursor-pointer"><i class="fa-solid fa-trash mr-1"></i>Delete</button>
        </td>
      </tr>`;
  }).join('');
}

function initSkillSortable() {
  if (skillSortable) skillSortable.destroy();
  skillSortable = new Sortable(skillsTableBody, {
    animation: 150,
    handle: '.drag-handle',
    ghostClass: 'sortable-ghost',
    dragClass: 'sortable-drag',
    onEnd: handleSkillReorder
  });
}

async function handleSkillReorder() {
  const rows = Array.from(skillsTableBody.querySelectorAll('tr[data-skill-id]'));

  const items = rows.map((r, i) => ({
    skillId: parseInt(r.dataset.skillId, 10),
    displayOrder: i + 1
  }));

  if (items.length === 0) return;

  showStatus('Saving new order...', false);

  try {
    const res = await fetchWithAuth('/api/Skill/Reorder', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(items)
    });

    if (!res.ok) {
      const data = await res.json().catch(() => null);
      const msg = extractErrorMessage(data, `Failed to save order (HTTP ${res.status}).`);
      showStatus(msg, true);
      await fetchSkills();
      return;
    }

    showStatus('Order updated successfully.', false);
    await fetchSkills();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Network error while saving order.', true);
    await fetchSkills();
  }
}

window.editSkill = function(id) {
  const s = skillsCache.find(x => String(x.skillId ?? x.id ?? x.SkillId) === String(id));
  if (s) openSkillModal(s);
  else showStatus('Skill not found.', true);
};

window.deleteSkill = async function(id) {
  const confirmed = await showConfirmModal({
    title: 'Delete Skill',
    message: 'Are you sure you want to delete this skill? This action cannot be undone.',
    confirmText: 'Delete', isDestructive: true
  });
  if (!confirmed) return;
  clearStatus();
  try {
    const res = await fetchWithAuth(`/api/Skill/Delete/${id}`, { method: 'DELETE' });
    if (!res.ok) {
      const data = await res.json().catch(() => null);
      showStatus(extractErrorMessage(data, `Failed to delete skill (HTTP ${res.status}).`), true);
      return;
    }
    showStatus('Skill deleted successfully.', false);
    await fetchSkills();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Network error while deleting skill.', true);
  }
};

skillForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  clearStatus();
  clearSkillModalStatus();

  const id = skillIdInput.value.trim();
  const skillName = skillNameInput.value.trim();
  const skillCategory = skillCategoryInput.value.trim();
  const skillLevel = parseInt(skillLevelInput.value, 10);

  if (!skillName || !skillCategory || isNaN(skillLevel) || skillLevel < 1 || skillLevel > 5) {
    showSkillModalStatus('Name, category, and a valid level (1-5) are required.', true);
    return;
  }

  const payload = { skillName, skillCategory, skillLevel };

  const isEdit = !!id;
  const url = isEdit ? `/api/Skill/Update/${id}` : '/api/Skill/Add';
  const method = isEdit ? 'PUT' : 'POST';

  const originalText = skillModalSaveBtn.textContent;
  skillModalSaveBtn.disabled = true;
  skillModalSaveBtn.classList.add('opacity-70', 'cursor-not-allowed');
  skillModalSaveBtn.textContent = isEdit ? 'Updating...' : 'Saving...';

  try {
    const res = await fetchWithAuth(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json().catch(() => null);
    if (!res.ok) {
      showSkillModalStatus(extractErrorMessage(data, isEdit ? `Failed to update skill (HTTP ${res.status}).` : `Failed to add skill (HTTP ${res.status}).`), true);
      return;
    }
    closeSkillModal();
    showStatus(isEdit ? 'Skill updated successfully.' : 'Skill added successfully.', false);
    await fetchSkills();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showSkillModalStatus(`Error: ${err.message || 'Network error.'}`, true);
  } finally {
    skillModalSaveBtn.disabled = false;
    skillModalSaveBtn.classList.remove('opacity-70', 'cursor-not-allowed');
    skillModalSaveBtn.textContent = originalText;
  }
});

addSkillBtn.addEventListener('click', () => openSkillModal());
skillModalCloseBtn.addEventListener('click', closeSkillModal);
skillModalCancelBtn.addEventListener('click', closeSkillModal);
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && !skillModal.classList.contains('hidden')) closeSkillModal();
});
skillModal.addEventListener('mousedown', (e) => { mouseDownOnSkillBackdrop = (e.target === skillModal); });
skillModal.addEventListener('mouseup', (e) => {
  if (mouseDownOnSkillBackdrop && e.target === skillModal) closeSkillModal();
  mouseDownOnSkillBackdrop = false;
});

// ---------- Social Links ----------
function openSocialLinkModal(link = null) {
  socialLinkForm.reset();
  clearSocialLinkModalStatus();
  if (link) {
    const id = link.socialLinkId ?? link.id ?? link.SocialLinkId ?? '';
    socialLinkModalTitle.textContent = 'Edit Social Link';
    socialLinkIdInput.value = id;
    socialLinkPlatformInput.value = link.platform || link.Platform || '';
    socialLinkUrlInput.value = link.url || link.Url || '';
  } else {
    socialLinkModalTitle.textContent = 'Add Social Link';
    socialLinkIdInput.value = '';
  }
  socialLinkModal.classList.remove('hidden');
}

function closeSocialLinkModal() {
  socialLinkModal.classList.add('hidden');
  clearSocialLinkModalStatus();
  socialLinkForm.reset();
}

async function fetchSocialLinks() {
  socialLinksTableBody.innerHTML = `<tr><td colspan="4" class="py-6 px-4 text-center text-slate-500">Loading social links...</td></tr>`;
  try {
    const res = await fetchWithAuth('/api/SocialLink/Get/All');
    if (!res.ok) {
      showStatus('Failed to load social links.', true);
      socialLinksTableBody.innerHTML = `<tr><td colspan="4" class="py-6 px-4 text-center text-rose-400">Error loading social links.</td></tr>`;
      return;
    }
    const data = await res.json();
    let links = [];
    if (Array.isArray(data)) links = data;
    else if (Array.isArray(data?.socialLinks)) links = data.socialLinks;
    else if (Array.isArray(data?.data)) links = data.data;
    else if (Array.isArray(data?.items)) links = data.items;

    links.sort((a, b) => {
      const aOrder = Number(a.displayOrder ?? a.DisplayOrder ?? 0);
      const bOrder = Number(b.displayOrder ?? b.DisplayOrder ?? 0);
      if (aOrder !== bOrder) return aOrder - bOrder;
      const aId = Number(a.socialLinkId ?? a.id ?? a.SocialLinkId ?? 0);
      const bId = Number(b.socialLinkId ?? b.id ?? b.SocialLinkId ?? 0);
      return bId - aId;
    });

    socialLinksCache = links;
    renderSocialLinksTable(links);
    initSocialLinkSortable();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Error connecting to social link service.', true);
  }
}

function renderSocialLinksTable(links) {
  if (!links || links.length === 0) {
    socialLinksTableBody.innerHTML = `<tr><td colspan="4" class="py-6 px-4 text-center text-slate-400">No social links found. Add one above!</td></tr>`;
    return;
  }
  socialLinksTableBody.innerHTML = links.map(link => {
    const id = link.socialLinkId ?? link.id ?? link.SocialLinkId;
    const platform = link.platform || link.Platform || 'Platform';
    const url = link.url || link.Url || '#';

    return `
      <tr data-social-link-id="${id}" class="hover:bg-slate-900/60 transition-colors">
        <td class="py-3 px-2 w-10 text-center">
          <i class="fa-solid fa-grip-vertical text-slate-500 hover:text-teal-400 cursor-grab drag-handle transition-colors"></i>
        </td>
        <td class="py-3 px-4 font-medium text-white max-w-[200px] truncate" title="${escapeHtml(platform)}">
          <span class="inline-block py-0.5 px-2 rounded-md bg-slate-800 border border-slate-700 text-xs text-slate-200">${escapeHtml(platform)}</span>
        </td>
        <td class="py-3 px-4 text-slate-400 max-w-[300px] truncate" title="${escapeHtml(url)}">
          <a href="${escapeHtml(url)}" target="_blank" rel="noopener noreferrer" class="text-teal-400 hover:underline inline-flex items-center gap-1.5">
            <span>${escapeHtml(url)}</span>
            <i class="fa-solid fa-arrow-up-right-from-square text-[10px]"></i>
          </a>
        </td>
        <td class="py-3 px-4 text-right space-x-2 whitespace-nowrap">
          <button type="button" onclick="editSocialLink('${id}')" class="py-1 px-2.5 rounded bg-slate-800 hover:bg-slate-700 text-teal-400 text-xs font-medium transition-colors cursor-pointer"><i class="fa-solid fa-pen-to-square mr-1"></i>Edit</button>
          <button type="button" onclick="deleteSocialLink('${id}')" class="py-1 px-2.5 rounded bg-slate-800 hover:bg-rose-950/60 text-rose-400 text-xs font-medium transition-colors cursor-pointer"><i class="fa-solid fa-trash mr-1"></i>Delete</button>
        </td>
      </tr>`;
  }).join('');
}

function initSocialLinkSortable() {
  if (socialLinkSortable) socialLinkSortable.destroy();
  socialLinkSortable = new Sortable(socialLinksTableBody, {
    animation: 150,
    handle: '.drag-handle',
    ghostClass: 'sortable-ghost',
    dragClass: 'sortable-drag',
    onEnd: handleSocialLinkReorder
  });
}

async function handleSocialLinkReorder() {
  const rows = Array.from(socialLinksTableBody.querySelectorAll('tr[data-social-link-id]'));

  const items = rows.map((r, i) => ({
    socialLinkId: parseInt(r.dataset.socialLinkId, 10),
    displayOrder: i + 1
  }));

  if (items.length === 0) return;

  showStatus('Saving new order...', false);

  try {
    const res = await fetchWithAuth('/api/SocialLink/Reorder', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(items)
    });

    if (!res.ok) {
      const data = await res.json().catch(() => null);
      const msg = extractErrorMessage(data, `Failed to save order (HTTP ${res.status}).`);
      showStatus(msg, true);
      await fetchSocialLinks();
      return;
    }

    showStatus('Order updated successfully.', false);
    await fetchSocialLinks();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Network error while saving order.', true);
    await fetchSocialLinks();
  }
}

window.editSocialLink = function(id) {
  const link = socialLinksCache.find(x => String(x.socialLinkId ?? x.id ?? x.SocialLinkId) === String(id));
  if (link) openSocialLinkModal(link);
  else showStatus('Social link not found.', true);
};

window.deleteSocialLink = async function(id) {
  const confirmed = await showConfirmModal({
    title: 'Delete Social Link',
    message: 'Are you sure you want to delete this social link? This action cannot be undone.',
    confirmText: 'Delete',
    isDestructive: true
  });
  if (!confirmed) return;
  clearStatus();
  try {
    const res = await fetchWithAuth(`/api/SocialLink/Delete/${id}`, { method: 'DELETE' });
    if (!res.ok) {
      const data = await res.json().catch(() => null);
      showStatus(extractErrorMessage(data, `Failed to delete social link (HTTP ${res.status}).`), true);
      return;
    }
    showStatus('Social link deleted successfully.', false);
    await fetchSocialLinks();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Network error while deleting social link.', true);
  }
};

socialLinkForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  clearStatus();
  clearSocialLinkModalStatus();

  const id = socialLinkIdInput.value.trim();
  const platform = socialLinkPlatformInput.value.trim();
  const url = socialLinkUrlInput.value.trim();

  if (!platform || !url) {
    showSocialLinkModalStatus('Platform and URL are required.', true);
    return;
  }

  const payload = { platform, url };

  const isEdit = !!id;
  const requestUrl = isEdit ? `/api/SocialLink/Update/${id}` : '/api/SocialLink/Add';
  const method = isEdit ? 'PUT' : 'POST';

  const originalText = socialLinkModalSaveBtn.textContent;
  socialLinkModalSaveBtn.disabled = true;
  socialLinkModalSaveBtn.classList.add('opacity-70', 'cursor-not-allowed');
  socialLinkModalSaveBtn.textContent = isEdit ? 'Updating...' : 'Saving...';

  try {
    const res = await fetchWithAuth(requestUrl, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json().catch(() => null);
    if (!res.ok) {
      showSocialLinkModalStatus(extractErrorMessage(data, isEdit ? `Failed to update social link (HTTP ${res.status}).` : `Failed to add social link (HTTP ${res.status}).`), true);
      return;
    }
    closeSocialLinkModal();
    showStatus(isEdit ? 'Social link updated successfully.' : 'Social link added successfully.', false);
    await fetchSocialLinks();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showSocialLinkModalStatus(`Error: ${err.message || 'Network error.'}`, true);
  } finally {
    socialLinkModalSaveBtn.disabled = false;
    socialLinkModalSaveBtn.classList.remove('opacity-70', 'cursor-not-allowed');
    socialLinkModalSaveBtn.textContent = originalText;
  }
});

addSocialLinkBtn.addEventListener('click', () => openSocialLinkModal());
socialLinkModalCloseBtn.addEventListener('click', closeSocialLinkModal);
socialLinkModalCancelBtn.addEventListener('click', closeSocialLinkModal);
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && !socialLinkModal.classList.contains('hidden')) closeSocialLinkModal();
});
socialLinkModal.addEventListener('mousedown', (e) => { mouseDownOnSocialLinkBackdrop = (e.target === socialLinkModal); });
socialLinkModal.addEventListener('mouseup', (e) => {
  if (mouseDownOnSocialLinkBackdrop && e.target === socialLinkModal) closeSocialLinkModal();
  mouseDownOnSocialLinkBackdrop = false;
});

// ---------- Profile ----------
async function fetchProfile() {
  try {
    const res = await fetchWithAuth('/api/PortfolioProfile/Get/Profile');
    if (!res.ok) {
      if (res.status === 404) {
        profileCache = null;
        if (profileCard) profileCard.classList.add('hidden');
        if (profileEmptyState) profileEmptyState.classList.remove('hidden');
        return;
      }
      showStatus('Failed to load profile.', true);
      return;
    }
    const data = await res.json();
    profileCache = data;

    if (!data) {
      if (profileCard) profileCard.classList.add('hidden');
      if (profileEmptyState) profileEmptyState.classList.remove('hidden');
      return;
    }

    if (profileCard) profileCard.classList.remove('hidden');
    if (profileEmptyState) profileEmptyState.classList.add('hidden');

    const fullName = data.fullName || data.FullName || '-';
    const title = data.professionalTitle || data.ProfessionalTitle || '-';
    const email = data.email || data.Email || '-';
    const location = data.location || data.Location || '-';
    const imageUrl = data.profileImageUrl || data.ProfileImageUrl || '-';
    const bio = data.bio || data.Bio || '-';

    if (profileViewFullname) profileViewFullname.textContent = fullName;
    if (profileViewTitle) profileViewTitle.textContent = title;
    if (profileViewEmail) profileViewEmail.textContent = email;
    if (profileViewLocation) profileViewLocation.textContent = location;
    if (profileViewImage) profileViewImage.textContent = imageUrl;
    if (profileViewBio) profileViewBio.textContent = bio;
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showStatus('Error connecting to profile service.', true);
  }
}

function openProfileModal() {
  profileForm.reset();
  clearProfileModalStatus();
  if (profileCache) {
    const id = profileCache.portfolioProfileId
      ?? profileCache.PortfolioProfileId
      ?? profileCache.profileId
      ?? profileCache.ProfileId
      ?? profileCache.id
      ?? '';

    profileIdInput.value = id;
    profileFullnameInput.value = profileCache.fullName || profileCache.FullName || '';
    profileTitleInput.value = profileCache.professionalTitle || profileCache.ProfessionalTitle || '';
    profileEmailInput.value = profileCache.email || profileCache.Email || '';
    profileLocationInput.value = profileCache.location || profileCache.Location || '';
    profileImageUrlInput.value = profileCache.profileImageUrl || profileCache.ProfileImageUrl || '';
    profileBioInput.value = profileCache.bio || profileCache.Bio || '';
  } else {
    profileIdInput.value = '';
  }
  profileModal.classList.remove('hidden');
}

function closeProfileModal() {
  profileModal.classList.add('hidden');
  clearProfileModalStatus();
  profileForm.reset();
}

profileForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  clearStatus();
  clearProfileModalStatus();

  const id = profileIdInput.value.trim();
  const fullName = profileFullnameInput.value.trim();
  const professionalTitle = profileTitleInput.value.trim();
  const email = profileEmailInput.value.trim();
  const location = profileLocationInput.value.trim();
  const profileImageUrl = profileImageUrlInput.value.trim() || null;
  const bio = profileBioInput.value.trim();

  if (!id) {
    showProfileModalStatus('Profile ID is missing. Please refresh the page.', true);
    return;
  }

  if (!fullName || !professionalTitle || !email || !location || !bio) {
    showProfileModalStatus('Full name, professional title, email, location, and bio are required.', true);
    return;
  }

  const payload = { fullName, professionalTitle, bio, profileImageUrl, email, location };

  const originalText = profileModalSaveBtn.textContent;
  profileModalSaveBtn.disabled = true;
  profileModalSaveBtn.classList.add('opacity-70', 'cursor-not-allowed');
  profileModalSaveBtn.textContent = 'Saving...';

  try {
    const url = `/api/PortfolioProfile/Update/${id}`;
    const res = await fetchWithAuth(url, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    const data = await res.json().catch(() => null);
    if (!res.ok) {
      showProfileModalStatus(extractErrorMessage(data, `Failed to update profile (HTTP ${res.status}).`), true);
      return;
    }
    closeProfileModal();
    showStatus('Profile updated successfully.', false);
    await fetchProfile();
  } catch (err) {
    if (err.message === 'SESSION_EXPIRED') return;
    showProfileModalStatus(`Error: ${err.message || 'Network error.'}`, true);
  } finally {
    profileModalSaveBtn.disabled = false;
    profileModalSaveBtn.classList.remove('opacity-70', 'cursor-not-allowed');
    profileModalSaveBtn.textContent = originalText;
  }
});

editProfileBtn.addEventListener('click', openProfileModal);
profileModalCloseBtn.addEventListener('click', closeProfileModal);
profileModalCancelBtn.addEventListener('click', closeProfileModal);
document.addEventListener('keydown', (e) => {
  if (e.key === 'Escape' && !profileModal.classList.contains('hidden')) closeProfileModal();
});
profileModal.addEventListener('mousedown', (e) => { mouseDownOnProfileBackdrop = (e.target === profileModal); });
profileModal.addEventListener('mouseup', (e) => {
  if (mouseDownOnProfileBackdrop && e.target === profileModal) closeProfileModal();
  mouseDownOnProfileBackdrop = false;
});

// ---------- View Toggles ----------
function showDashboard() {
  loginView.classList.add('hidden');
  dashboardView.classList.remove('hidden');
  switchTab('projects');
  fetchProjects();
}
function showLogin() {
  dashboardView.classList.add('hidden');
  loginView.classList.remove('hidden');
}
function checkAuth() {
  if (localStorage.getItem('accessToken')) showDashboard();
  else showLogin();
}

// ---------- Password Toggle ----------
if (togglePasswordBtn && togglePasswordIcon) {
  togglePasswordBtn.addEventListener('click', () => {
    const isHidden = passwordInput.type === 'password';
    passwordInput.type = isHidden ? 'text' : 'password';
    togglePasswordIcon.className = isHidden ? 'fa-solid fa-eye-slash' : 'fa-solid fa-eye';
  });
}

// ---------- Login ----------
loginForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  clearStatus();
  const email = emailInput.value.trim();
  const password = passwordInput.value;
  if (!email || !password) { showStatus('Please enter both email and password.'); return; }
  submitBtn.disabled = true;
  submitBtn.classList.add('opacity-70', 'cursor-not-allowed');
  try {
    const res = await fetch('/api/Auth/Login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    const data = await res.json().catch(() => null);
    if (!res.ok) { showStatus(extractErrorMessage(data, 'Login failed.')); return; }
    const accessToken = data?.accessToken || data?.AccessToken;
    const refreshToken = data?.refreshToken || data?.RefreshToken;
    if (!accessToken) { showStatus('Invalid server response: Access token missing.'); return; }
    localStorage.setItem('accessToken', accessToken);
    if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
    loginForm.reset();
    showDashboard();
  } catch (err) {
    showStatus('Network error or server unavailable.');
  } finally {
    submitBtn.disabled = false;
    submitBtn.classList.remove('opacity-70', 'cursor-not-allowed');
  }
});

// ---------- Tabs & Logout ----------
logoutBtn.addEventListener('click', showLogoutConfirm);
tabProjects.addEventListener('click', () => switchTab('projects'));
tabSkills.addEventListener('click', () => switchTab('skills'));
tabSocialLinks.addEventListener('click', () => switchTab('social-links'));
tabProfile.addEventListener('click', () => switchTab('profile'));

// ---------- Init ----------
injectLogoutModal();
checkAuth();