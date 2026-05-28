(() => {
  'use strict';

  /* ── Theme ──────────────────────────────────────────────────────────── */
  const THEME_KEY = 'sr-theme';
  const htmlEl    = document.documentElement;
  const themeBtn  = document.getElementById('themeToggle');

  function applyTheme(theme) {
    htmlEl.setAttribute('data-bs-theme', theme);
    if (themeBtn) {
      themeBtn.querySelector('i').className =
        theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
    }
  }

  function initTheme() {
    const saved = localStorage.getItem(THEME_KEY);
    const pref  = saved || (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
    applyTheme(pref);
  }

  if (themeBtn) {
    themeBtn.addEventListener('click', () => {
      const next = htmlEl.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
      localStorage.setItem(THEME_KEY, next);
      applyTheme(next);
    });
  }

  initTheme();

  /* ── Navbar scroll shadow ────────────────────────────────────────────── */
  const navbar = document.querySelector('.navbar');
  if (navbar) {
    const onScroll = () =>
      navbar.classList.toggle('scrolled', window.scrollY > 20);
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
  }

  /* ── Active nav link ─────────────────────────────────────────────────── */
  (function markActiveNav() {
    const page = location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
      const href = link.getAttribute('href') || '';
      if (href === page || (page === '' && href === 'index.html')) {
        link.classList.add('active');
        link.setAttribute('aria-current', 'page');
      }
    });
  })();

  /* ── Collapse navbar on mobile link click ───────────────────────────── */
  document.querySelectorAll('.navbar-nav .nav-link').forEach(link => {
    link.addEventListener('click', () => {
      const toggler = document.querySelector('.navbar-toggler');
      const collapse = document.getElementById('navbarNav');
      if (collapse && collapse.classList.contains('show') && toggler) {
        toggler.click();
      }
    });
  });

  /* ── Skill bars (IntersectionObserver) ──────────────────────────────── */
  function animateSkillBars(entries, observer) {
    entries.forEach(entry => {
      if (!entry.isIntersecting) return;
      entry.target.querySelectorAll('.skill-fill').forEach(bar => {
        const w = bar.dataset.width || 0;
        bar.style.width = w + '%';
      });
      observer.unobserve(entry.target);
    });
  }

  const skillSections = document.querySelectorAll('.skills-section');
  if (skillSections.length) {
    const obs = new IntersectionObserver(animateSkillBars, { threshold: 0.15 });
    skillSections.forEach(s => obs.observe(s));
  } else {
    // Fallback: animate any visible skill bars on DOMContentLoaded
    document.querySelectorAll('.skill-fill').forEach(bar => {
      bar.style.width = (bar.dataset.width || 0) + '%';
    });
  }

  /* ── Scroll reveal ───────────────────────────────────────────────────── */
  const revealObs = new IntersectionObserver(entries => {
    entries.forEach(e => {
      if (e.isIntersecting) {
        e.target.classList.add('visible');
        revealObs.unobserve(e.target);
      }
    });
  }, { threshold: 0.08 });

  document.querySelectorAll('.animate-up').forEach(el => revealObs.observe(el));

  /* ── Project filter ──────────────────────────────────────────────────── */
  const filterPills = document.querySelectorAll('.filter-pill');
  const projectCards = document.querySelectorAll('.project-card-wrap');

  if (filterPills.length && projectCards.length) {
    filterPills.forEach(pill => {
      pill.addEventListener('click', () => {
        filterPills.forEach(p => p.classList.remove('active'));
        pill.classList.add('active');
        const filter = pill.dataset.filter;

        projectCards.forEach(card => {
          const tags = (card.dataset.tags || '').toLowerCase();
          const show = filter === 'all' || tags.includes(filter.toLowerCase());
          card.style.display = show ? '' : 'none';
        });
      });
    });
  }

  /* ── Contact form (Formspree) ────────────────────────────────────────── */
  const contactForm = document.getElementById('contactForm');
  const formStatus  = document.getElementById('formStatus');

  if (contactForm) {
    contactForm.addEventListener('submit', async e => {
      e.preventDefault();

      const fields = contactForm.querySelectorAll('input[required], textarea[required]');
      let valid = true;
      fields.forEach(f => {
        f.classList.remove('is-invalid');
        if (!f.value.trim()) { f.classList.add('is-invalid'); valid = false; }
      });
      const emailField = contactForm.querySelector('input[type="email"]');
      if (emailField && emailField.value.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailField.value.trim())) {
        emailField.classList.add('is-invalid');
        valid = false;
      }
      if (!valid) {
        showFormStatus('danger', '<i class="bi bi-exclamation-triangle-fill me-2"></i>Please fill in all required fields with valid information.');
        return;
      }

      const btn = contactForm.querySelector('button[type="submit"]');
      const orig = btn.innerHTML;
      btn.disabled = true;
      btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Sending…';

      try {
        const res = await fetch(contactForm.action, {
          method: 'POST',
          body: new FormData(contactForm),
          headers: { Accept: 'application/json' }
        });

        if (res.ok) {
          contactForm.reset();
          showFormStatus('success',
            '<i class="bi bi-check-circle-fill me-2"></i>Message sent! I\'ll get back to you soon.');
        } else {
          showFormStatus('danger',
            '<i class="bi bi-exclamation-triangle-fill me-2"></i>Something went wrong. Please try again or email me directly.');
        }
      } catch {
        showFormStatus('danger',
          '<i class="bi bi-wifi-off me-2"></i>Network error. Please check your connection and try again.');
      } finally {
        btn.disabled = false;
        btn.innerHTML = orig;
      }
    });
  }

  function showFormStatus(type, html) {
    if (!formStatus) return;
    formStatus.className = `alert alert-${type} mt-3`;
    formStatus.innerHTML = html;
    formStatus.hidden = false;
    formStatus.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    setTimeout(() => { formStatus.hidden = true; }, 8000);
  }

  if (contactForm) {
    contactForm.querySelectorAll('input, textarea').forEach(f => {
      f.addEventListener('input', () => f.classList.remove('is-invalid'));
    });
  }

  /* ── Typed effect on hero (index only) ──────────────────────────────── */
  const typedEl = document.getElementById('typedTitle');
  if (typedEl) {
    const titles = [
      '.NET Backend Engineer',
      'ASP.NET Core Developer',
      'Enterprise App Developer',
      'Clean Architecture Advocate'
    ];
    let ti = 0, ci = 0, deleting = false;

    function type() {
      const cur = titles[ti];
      if (deleting) {
        typedEl.textContent = cur.substring(0, --ci);
        if (ci === 0) { deleting = false; ti = (ti + 1) % titles.length; setTimeout(type, 500); return; }
        setTimeout(type, 40);
      } else {
        typedEl.textContent = cur.substring(0, ++ci);
        if (ci === cur.length) { deleting = true; setTimeout(type, 2200); return; }
        setTimeout(type, 75);
      }
    }
    setTimeout(type, 800);
  }

})();
