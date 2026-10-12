// API Endpoints Configuration
const API_CONFIG = {
  profile: {
    primary: '/api/PortfolioProfile/Get/Profile'
  },
  projects: {
    primary: '/api/Project/Get/All'
  },
  skills: {
    primary: '/api/Skill/Get/All'
  },
  socialLinks: {
    primary: '/api/SocialLink/Get/All'
  }
};

// Application State
const state = {
  profile: null,
  projects: [],
  skills: [],
  socialLinks: []
};

// Safe HTML escaping
function escapeHtml(str) {
  if (str === null || str === undefined) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

// Sort helper — sorts by displayOrder ASC, then id DESC (newest first)
function sortByDisplayOrder(items, idKeys) {
  return items.sort((a, b) => {
    const aOrder = Number(a.displayOrder ?? a.DisplayOrder ?? 0);
    const bOrder = Number(b.displayOrder ?? b.DisplayOrder ?? 0);
    if (aOrder !== bOrder) return aOrder - bOrder;

    const aId = Number(idKeys.map(k => a[k]).find(v => v != null) ?? 0);
    const bId = Number(idKeys.map(k => b[k]).find(v => v != null) ?? 0);
    return bId - aId;
  });
}

// Fetch helper
async function fetchData(endpoint, label) {
  const res = await fetch(endpoint);
  if (!res.ok) {
    throw new Error(`${label}: HTTP ${res.status}`);
  }
  return await res.json();
}

// Icon helper
function getSocialIcon(platform) {
  const p = (platform || '').toLowerCase();
  if (p.includes('github')) return 'fa-brands fa-github';
  if (p.includes('linkedin')) return 'fa-brands fa-linkedin-in';
  if (p.includes('twitter') || p.includes('x.com')) return 'fa-brands fa-x-twitter';
  if (p.includes('youtube')) return 'fa-brands fa-youtube';
  if (p.includes('facebook')) return 'fa-brands fa-facebook';
  if (p.includes('instagram')) return 'fa-brands fa-instagram';
  if (p.includes('discord')) return 'fa-brands fa-discord';
  if (p.includes('medium')) return 'fa-brands fa-medium';
  if (p.includes('stack') || p.includes('stackoverflow')) return 'fa-brands fa-stack-overflow';
  if (p.includes('mail') || p.includes('email')) return 'fa-solid fa-envelope';
  if (p.includes('website') || p.includes('portfolio') || p.includes('globe')) return 'fa-solid fa-globe';
  return 'fa-solid fa-link';
}

// Navigation and smooth scrolling
function initNavigation() {
  const menuBtn = document.getElementById('mobile-menu-btn');
  const mobileMenu = document.getElementById('mobile-menu');
  const icon = document.getElementById('mobile-menu-icon');

  if (menuBtn && mobileMenu) {
    menuBtn.addEventListener('click', () => {
      const isExpanded = !mobileMenu.classList.contains('hidden');
      if (isExpanded) {
        mobileMenu.classList.add('hidden');
        icon.className = 'fa-solid fa-bars text-xl';
      } else {
        mobileMenu.classList.remove('hidden');
        icon.className = 'fa-solid fa-xmark text-xl';
      }
    });

    document.querySelectorAll('.mobile-nav-link').forEach(link => {
      link.addEventListener('click', () => {
        mobileMenu.classList.add('hidden');
        icon.className = 'fa-solid fa-bars text-xl';
      });
    });
  }

  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
      const href = this.getAttribute('href');
      if (href === '#' || !href) return;
      const target = document.querySelector(href);
      if (target) {
        e.preventDefault();
        target.scrollIntoView({
          behavior: 'smooth',
          block: 'start'
        });
        if (history.pushState) {
          history.pushState(null, null, href);
        }
      }
    });
  });

  const yearEl = document.getElementById('current-year');
  if (yearEl) {
    yearEl.textContent = new Date().getFullYear();
  }
}

// Status Banner
function showStatusBanner({ type, message, dismissible = false }) {
  const container = document.getElementById('status-box');
  if (!container) return;

  container.className = 'max-w-xl mx-auto shadow-xl pointer-events-auto rounded-lg p-3 sm:p-4 transition-all duration-300 ';

  if (type === 'loading') {
    container.className += 'bg-slate-900 border border-teal-500/50 text-teal-300';
    container.innerHTML = `
      <div class="flex items-center gap-3">
        <svg class="animate-spin h-5 w-5 text-teal-400 shrink-0" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"></path>
        </svg>
        <span class="text-sm font-medium text-slate-200">${escapeHtml(message)}</span>
      </div>
    `;
  } else if (type === 'error') {
    container.className += 'bg-rose-950/90 border border-rose-500/50 text-rose-200 backdrop-blur';
    container.innerHTML = `
      <div class="flex items-start gap-3 justify-between">
        <div class="flex items-start gap-3">
          <i class="fa-solid fa-triangle-exclamation text-rose-400 mt-0.5 text-lg shrink-0"></i>
          <div>
            <div class="text-sm font-semibold text-rose-100">Notice</div>
            <div class="text-xs text-rose-300 mt-0.5 leading-relaxed">${escapeHtml(message)}</div>
          </div>
        </div>
        ${dismissible ? `
          <button onclick="document.getElementById('status-box').classList.add('hidden')" class="text-rose-300 hover:text-white p-1 ml-2">
            <i class="fa-solid fa-xmark"></i>
          </button>
        ` : ''}
      </div>
    `;
  }

  container.classList.remove('hidden');
}

function hideStatusBanner() {
  const container = document.getElementById('status-box');
  if (container) {
    container.classList.add('hidden');
  }
}

// Render Profile
function renderProfile(profile) {
  const heroLoading = document.getElementById('hero-loading');
  const heroContent = document.getElementById('hero-content');

  if (heroLoading) heroLoading.classList.add('hidden');
  if (heroContent) heroContent.classList.remove('hidden');

  if (!profile) return;

  const fullName = profile.fullName || profile.FullName || 'Developer';
  const title = profile.professionalTitle || profile.ProfessionalTitle || 'Software Engineer';
  const bio = profile.bio || profile.Bio || '';
  const email = profile.email || profile.Email || 'contact@example.com';
  const location = profile.location || profile.Location || 'Remote / Worldwide';
  const imageUrl = profile.profileImageUrl || profile.ProfileImageUrl;

  document.title = `${fullName} | Portfolio`;
  const navBrand = document.getElementById('nav-brand-name');
  if (navBrand) navBrand.textContent = fullName;

  const heroNameEl = document.getElementById('hero-name');
  if (heroNameEl) heroNameEl.textContent = fullName;

  const heroTitleEl = document.getElementById('hero-title');
  if (heroTitleEl) heroTitleEl.textContent = title;

  const heroBioEl = document.getElementById('hero-bio-short');
  if (heroBioEl) {
    heroBioEl.textContent = bio || 'Crafting resilient, high-performance web applications and backend architectures.';
  }

  const avatarImg = document.getElementById('hero-avatar');
  const avatarFallback = document.getElementById('hero-avatar-fallback');
  if (imageUrl && avatarImg && avatarFallback) {
    avatarImg.src = imageUrl;
    avatarImg.alt = fullName;
    avatarImg.onload = () => {
      avatarImg.classList.remove('hidden');
      avatarFallback.classList.add('hidden');
    };
    avatarImg.onerror = () => {
      avatarImg.classList.add('hidden');
      avatarFallback.classList.remove('hidden');
    };
  }

  const aboutBioEl = document.getElementById('about-bio');
  if (aboutBioEl) {
    if (bio) {
      aboutBioEl.innerHTML = `<p class="whitespace-pre-line">${escapeHtml(bio)}</p>`;
    } else {
      aboutBioEl.innerHTML = `<p class="text-slate-400 italic">No biography provided yet.</p>`;
    }
  }

  const aboutLocationEl = document.getElementById('about-location');
  if (aboutLocationEl) aboutLocationEl.textContent = location;

  const aboutEmailEl = document.getElementById('about-email');
  if (aboutEmailEl) {
    aboutEmailEl.innerHTML = `<a href="mailto:${escapeHtml(email)}" class="hover:text-teal-400 transition-colors">${escapeHtml(email)}</a>`;
  }

  const aboutTitleEl = document.getElementById('about-title');
  if (aboutTitleEl) aboutTitleEl.textContent = title;

  const contactEmailEl = document.getElementById('contact-email');
  if (contactEmailEl) {
    contactEmailEl.innerHTML = `<a href="mailto:${escapeHtml(email)}" class="hover:text-teal-400 transition-colors">${escapeHtml(email)}</a>`;
  }

  const contactEmailBtn = document.getElementById('contact-email-btn');
  if (contactEmailBtn) {
    contactEmailBtn.href = `mailto:${encodeURIComponent(email)}?subject=Connecting%20via%20Portfolio`;
  }

  const footerCopy = document.getElementById('footer-copy');
  if (footerCopy) {
    const year = new Date().getFullYear();
    footerCopy.innerHTML = `&copy; ${year} ${escapeHtml(fullName)}. All rights reserved.`;
  }
}

// Render Projects
function renderProjects(projects) {
  const container = document.getElementById('projects-container');
  if (!container) return;

  if (!projects || projects.length === 0) {
    container.className = 'text-center py-12';
    container.innerHTML = `
      <div class="max-w-md mx-auto p-8 rounded-2xl bg-slate-900/60 border border-slate-800 text-slate-400">
        <i class="fa-solid fa-folder-open text-4xl text-teal-400/50 mb-3 block"></i>
        <p class="font-medium text-slate-300">No projects to showcase right now.</p>
        <p class="text-xs text-slate-500 mt-1">Check back soon for updates!</p>
      </div>
    `;
    return;
  }

  container.className = 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6';

  const cardsHtml = projects.map(proj => {
    const title = proj.title || proj.Title || 'Untitled Project';
    const desc = proj.description || proj.Description || 'No description available.';
    const img = proj.imageUrl || proj.ImageUrl;
    const github = proj.gitHubUrl || proj.GitHubUrl;
    const live = proj.liveUrl || proj.LiveUrl;

    const imgMarkup = img
      ? `<div class="relative h-48 w-full overflow-hidden bg-slate-950">
           <img src="${escapeHtml(img)}" alt="${escapeHtml(title)}" loading="lazy" decoding="async" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" onerror="this.parentElement.style.display='none'">
         </div>`
      : `<div class="h-32 w-full bg-gradient-to-br from-slate-900 to-slate-800/80 flex items-center justify-center text-teal-400/30 text-4xl">
           <i class="fa-solid fa-code"></i>
         </div>`;

    const linksMarkup = `
      <div class="pt-4 mt-auto border-t border-slate-800 flex items-center gap-3">
        ${live ? `
          <a href="${escapeHtml(live)}" target="_blank" rel="noopener noreferrer" class="inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-md bg-teal-500 hover:bg-teal-400 text-slate-950 transition-colors">
            <i class="fa-solid fa-arrow-up-right-from-square text-xs"></i> Live Demo
          </a>
        ` : ''}
        ${github ? `
          <a href="${escapeHtml(github)}" target="_blank" rel="noopener noreferrer" class="inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-md bg-slate-800 hover:bg-slate-700 text-slate-200 transition-colors border border-slate-700">
            <i class="fa-brands fa-github text-sm"></i> Code
          </a>
        ` : ''}
        ${!live && !github ? `<span class="text-xs text-slate-500 italic">Internal repository</span>` : ''}
      </div>
    `;

    return `
      <div class="group bg-slate-900/80 border border-slate-800 hover:border-teal-500/40 rounded-xl overflow-hidden flex flex-col transition-all duration-300 hover:shadow-xl hover:shadow-teal-500/5 hover:-translate-y-1">
        ${imgMarkup}
        <div class="p-6 flex flex-col flex-grow">
          <h3 class="text-lg font-bold text-white group-hover:text-teal-300 transition-colors mb-2">${escapeHtml(title)}</h3>
          <p class="text-sm text-slate-300 leading-relaxed mb-4 flex-grow line-clamp-4">${escapeHtml(desc)}</p>
          ${linksMarkup}
        </div>
      </div>
    `;
  }).join('');

  container.innerHTML = cardsHtml;
}

// Render Skills
function renderSkills(skills) {
  const container = document.getElementById('skills-container');
  if (!container) return;

  if (!skills || skills.length === 0) {
    container.innerHTML = `
      <div class="text-center py-12">
        <div class="max-w-md mx-auto p-8 rounded-2xl bg-slate-900/60 border border-slate-800 text-slate-400">
          <i class="fa-solid fa-screwdriver-wrench text-4xl text-teal-400/50 mb-3 block"></i>
          <p class="font-medium text-slate-300">No skills listed yet.</p>
        </div>
      </div>
    `;
    return;
  }

  const categories = {};
  skills.forEach(s => {
    const cat = s.skillCategory || s.SkillCategory || 'General';
    if (!categories[cat]) categories[cat] = [];
    categories[cat].push(s);
  });

  const categoriesHtml = Object.entries(categories).map(([category, items]) => {
    const itemsHtml = items.map(skill => {
      const name = skill.skillName || skill.SkillName || 'Skill';
      const level = Number(skill.skillLevel || skill.SkillLevel || 0);

      let dotsHtml = '';
      if (level > 0) {
        dotsHtml = `
          <div class="flex items-center gap-1 mt-1.5" title="Proficiency: ${level}/5">
            ${[1, 2, 3, 4, 5].map(i => `
              <span class="w-1.5 h-1.5 rounded-full ${i <= level ? 'bg-teal-400' : 'bg-slate-700'}"></span>
            `).join('')}
          </div>
        `;
      }

      return `
        <div class="p-3 rounded-lg bg-slate-950/70 border border-slate-800/80 hover:border-teal-500/30 transition-all flex flex-col justify-between">
          <span class="text-sm font-medium text-slate-200">${escapeHtml(name)}</span>
          ${dotsHtml}
        </div>
      `;
    }).join('');

    return `
      <div class="bg-slate-900/80 border border-slate-800 rounded-xl p-6 backdrop-blur">
        <h3 class="text-sm uppercase tracking-wider text-teal-400 font-semibold mb-4 flex items-center gap-2">
          <i class="fa-solid fa-terminal text-xs"></i>
          ${escapeHtml(category)}
        </h3>
        <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
          ${itemsHtml}
        </div>
      </div>
    `;
  }).join('');

  container.innerHTML = categoriesHtml;
}

// Render Social Links
function renderSocialLinks(links) {
  const heroContainer = document.getElementById('hero-socials');
  const contactContainer = document.getElementById('contact-socials');

  if (!links || links.length === 0) {
    if (contactContainer) {
      contactContainer.innerHTML = '<span class="text-xs text-slate-500">No social links configured.</span>';
    }
    return;
  }

  if (heroContainer) {
    heroContainer.innerHTML = links.map(link => {
      const platform = link.platform || link.Platform || 'Link';
      const url = link.url || link.Url || '#';
      const icon = getSocialIcon(platform);

      return `
        <a href="${escapeHtml(url)}" target="_blank" rel="noopener noreferrer" title="${escapeHtml(platform)}" class="w-10 h-10 rounded-lg bg-slate-900/80 border border-slate-800 text-slate-400 hover:text-teal-400 hover:border-teal-500/40 hover:bg-slate-800 flex items-center justify-center transition-all hover:scale-110 shadow-sm" aria-label="${escapeHtml(platform)}">
          <i class="${icon} text-lg"></i>
        </a>
      `;
    }).join('');
  }

  if (contactContainer) {
    contactContainer.innerHTML = links.map(link => {
      const platform = link.platform || link.Platform || 'Link';
      const url = link.url || link.Url || '#';
      const icon = getSocialIcon(platform);

      return `
        <a href="${escapeHtml(url)}" target="_blank" rel="noopener noreferrer" class="px-4 py-2.5 rounded-lg bg-slate-950 border border-slate-800/80 text-slate-300 hover:text-teal-400 hover:border-teal-500/40 hover:bg-slate-900 flex items-center gap-2.5 text-sm font-medium transition-all shadow-sm">
          <i class="${icon} text-base text-teal-400"></i>
          <span>${escapeHtml(platform)}</span>
        </a>
      `;
    }).join('');
  }
}

// Main Data Fetcher
async function loadPortfolioData() {
  showStatusBanner({ type: 'loading', message: 'Loading portfolio profile, projects, and skills...' });

  const results = await Promise.allSettled([
    fetchData(API_CONFIG.profile.primary, 'Profile'),
    fetchData(API_CONFIG.projects.primary, 'Projects'),
    fetchData(API_CONFIG.skills.primary, 'Skills'),
    fetchData(API_CONFIG.socialLinks.primary, 'Social Links')
  ]);

  const [profileRes, projectsRes, skillsRes, socialsRes] = results;

  const failed = [];

  if (profileRes.status === 'fulfilled') {
    state.profile = profileRes.value;
    renderProfile(state.profile);
  } else {
    failed.push(`Profile: ${profileRes.reason?.message || 'Failed'}`);
    renderProfile(null);
  }

  if (projectsRes.status === 'fulfilled') {
    let projects = Array.isArray(projectsRes.value) ? projectsRes.value : [];
    projects = sortByDisplayOrder(projects, ['projectId', 'id', 'ProjectId']);
    state.projects = projects;
    renderProjects(state.projects);
  } else {
    failed.push(`Projects: ${projectsRes.reason?.message || 'Failed'}`);
    renderProjects([]);
  }

  if (skillsRes.status === 'fulfilled') {
    let skills = Array.isArray(skillsRes.value) ? skillsRes.value : [];
    skills = sortByDisplayOrder(skills, ['skillId', 'id', 'SkillId']);
    state.skills = skills;
    renderSkills(state.skills);
  } else {
    failed.push(`Skills: ${skillsRes.reason?.message || 'Failed'}`);
    renderSkills([]);
  }

  if (socialsRes.status === 'fulfilled') {
    let links = Array.isArray(socialsRes.value) ? socialsRes.value : [];
    links = sortByDisplayOrder(links, ['socialLinkId', 'id', 'SocialLinkId']);
    state.socialLinks = links;
    renderSocialLinks(state.socialLinks);
  } else {
    failed.push(`Social Links: ${socialsRes.reason?.message || 'Failed'}`);
    renderSocialLinks([]);
  }

  if (failed.length === 0) {
    hideStatusBanner();
  } else {
    showStatusBanner({
      type: 'error',
      message: `Failed to load: ${failed.join(' | ')}. Please check the backend endpoints.`,
      dismissible: true
    });
  }
}

document.addEventListener('DOMContentLoaded', () => {
  initNavigation();
  loadPortfolioData();
});