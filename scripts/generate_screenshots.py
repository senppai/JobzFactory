# -*- coding: utf-8 -*-
"""Generate app screenshots for the JobzFactory report.

Builds standalone HTML mockups that reuse the repo's real CSS files (inlined),
mirror the real Razor views' structure, and screenshots them headless with
Microsoft Edge into scripts/report_assets/fig_NN.png (figure numbers 39..58).

Edge is used instead of Playwright to avoid a heavy install; it renders the
real CSS faithfully. Font Awesome / IcoFont icons are substituted with emoji
because the headless capture runs without network.
"""
import os
import shutil
import subprocess

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
ASSETS = os.path.join(os.path.dirname(__file__), "report_assets")
MOCK = os.path.join(ASSETS, "mockups")
os.makedirs(MOCK, exist_ok=True)

EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"


def read(*path):
    with open(os.path.join(ROOT, *path), "r", encoding="utf-8", errors="replace") as f:
        return f.read()


CSS_INDEX = read("JobzFactory", "css", "index-modern.css")
CSS_NAV = read("JobzFactory", "css", "nav-modern.css")
CSS_JOB = read("JobzFactory", "css", "job-modern.css")
CSS_FOOTER = read("JobzFactory", "css", "footer-modern.css")
CSS_RC = read("Recruteur", "assets", "css", "recruteur-modern.css")
CSS_AD = read("Administration", "Content", "admin-modern.css")
CSS_PROFIL = read("Profil", "Css", "profil-modern.css")

# Minimal Bootstrap-like container + nav shim (the real layout uses Bootstrap).
SHIM = """
* { box-sizing: border-box; }
body { margin: 0; font-family: "Plus Jakarta Sans","Nunito Sans",-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif; }
.container { width: 100%; max-width: 1140px; margin: 0 auto; padding: 0 1rem; }
.row { display: flex; flex-wrap: wrap; margin: 0 -0.5rem; }
.col-md-6 { flex: 0 0 50%; max-width: 50%; padding: 0 0.5rem; }
.jf-topbar {
  position: relative; z-index: 20;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
  padding: 0.9rem 0;
}
.jf-topbar__inner { display: flex; align-items: center; gap: 1.5rem; }
.jf-topbar__brand { color: #fff; font-weight: 800; font-size: 1.25rem; letter-spacing: -0.02em; }
.jf-topbar__brand b { color: #a5b4fc; }
.jf-topbar__links { display: flex; gap: 1.25rem; margin-left: 1rem; }
.jf-topbar__links a { color: #cbd5e1; text-decoration: none; font-weight: 600; font-size: 0.9rem; }
.jf-topbar__links a:hover { color: #fff; }
.jf-topbar__actions { margin-left: auto; display: flex; gap: 0.6rem; }
.jf-footer { background: #0f172a; color: #94a3b8; padding: 2.5rem 0; font-size: 0.9rem; }
.jf-footer__grid { display: grid; grid-template-columns: 2fr 1fr 1fr; gap: 2rem; }
.jf-footer h4 { color: #fff; font-size: 1rem; margin: 0 0 0.75rem; }
.jf-footer a { color: #94a3b8; text-decoration: none; display: block; padding: 0.2rem 0; }
.jf-footer__brand { color: #fff; font-weight: 800; font-size: 1.15rem; margin-bottom: 0.5rem; }
.jf-footer__brand b { color: #a5b4fc; }
.form-control { width: 100%; padding: 0.8rem 1rem; border: 1px solid #e2e8f0; border-radius: 12px; font-size: 0.95rem; color: #0f172a; background: #fff; }
"""

HTML_HEAD = """<!DOCTYPE html><html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=1280, initial-scale=1">
<style>%s</style></head><body>%s</body></html>"""

# ---------------------------------------------------------------------------
# Public site mockups
# ---------------------------------------------------------------------------

def topbar(active="Home"):
    links = [("Home", "#"), ("Jobs", "#jobs"), ("Recruiters", "#"), ("About", "#")]
    items = "".join(
        '<a href="%s" style="%s">%s</a>' % (h, "color:#fff;" if h == active else "", h)
        for h, href in links)
    return (
        '<header class="jf-topbar"><div class="container jf-topbar__inner">'
        '<div class="jf-topbar__brand">Jobz<b>Factory</b></div>'
        '<nav class="jf-topbar__links">%s</nav>'
        '<div class="jf-topbar__actions">'
        '<a class="jf-btn jf-btn--ghost jf-nav-btn jf-nav-btn--employer" href="#">For employers</a>'
        '<a class="jf-btn jf-btn--primary jf-nav-btn jf-nav-btn--profile" href="#">My Profile</a>'
        '</div></div></header>' % items)


def footer():
    return (
        '<footer class="jf-footer"><div class="container jf-footer__grid">'
        '<div><div class="jf-footer__brand">Jobz<b>Factory</b></div>'
        '<p>Find your next career move. Browse curated opportunities from top employers across Morocco.</p></div>'
        '<div><h4>Candidates</h4><a>Browse jobs</a><a>My profile</a><a>My applications</a></div>'
        '<div><h4>Recruiters</h4><a>Post a job</a><a>Dashboard</a><a>Contact</a></div>'
        '</div></footer>')


JOBS = [
    ("Senior Frontend Developer", "InnovaTech", "Casablanca", "IT / Software", "A"),
    ("Data Analyst", "DataCorp", "Rabat", "Data / BI", "D"),
    ("Marketing Manager", "BrandWave", "Casablanca", "Marketing", "B"),
    ("DevOps Engineer", "CloudNine", "Marrakech", "IT / Software", "C"),
    ("Accountant", "FinTrust", "Tanger", "Finance", "F"),
    ("UX Designer", "PixelLab", "Casablanca", "Design", "P"),
]


def job_cards():
    cards = []
    for title, company, city, sector, ini in JOBS:
        cards.append(
            '<article class="jf-job-card">'
            '<div class="jf-job-card__avatar">%s</div>'
            '<div class="jf-job-card__body">'
            '<div class="jf-job-card__top"><h3 class="jf-job-card__title">%s</h3>'
            '<span class="jf-job-card__badge">%s</span></div>'
            '<div class="jf-job-card__meta">'
            '<span>🏢 %s</span><span>📍 %s, Morocco</span>'
            '</div></div>'
            '<div class="jf-job-card__action"><a class="jf-btn jf-btn--primary">View &amp; Apply</a></div>'
            '</article>' % (ini, title, sector, company, city))
    return cards


def html_home():
    body = topbar("Home") + """
    <div class="jf-home">
      <section class="jf-hero">
        <div class="jf-hero__bg" style="background:linear-gradient(135deg,rgba(15,23,42,.92),rgba(30,41,59,.78) 45%,rgba(79,70,229,.55));"></div>
        <div class="jf-hero__overlay"></div>
        <div class="container jf-hero__content">
          <span class="jf-hero__badge">Now hiring across Morocco</span>
          <h1 class="jf-hero__title">Your next career move <span>starts here</span></h1>
          <p class="jf-hero__subtitle">Discover curated opportunities from top employers. Browse open roles, explore companies, and apply in just a few clicks.</p>
          <div class="jf-hero__actions">
            <a href="#jobs" class="jf-btn jf-btn--primary">💼 Browse 142 openings</a>
            <a class="jf-btn jf-btn--ghost">👤 My Profile</a>
          </div>
          <div class="jf-hero__stats">
            <div class="jf-stat"><strong>142</strong><span>Open positions</span></div>
            <div class="jf-stat"><strong>6</strong><span>On this page</span></div>
            <div class="jf-stat"><strong>24</strong><span>Pages</span></div>
          </div>
        </div>
      </section>
      <section id="jobs" class="jf-jobs">
        <div class="container">
          <header class="jf-jobs__header">
            <div><h2>Latest opportunities</h2><p>Hand-picked roles updated regularly from trusted recruiters.</p></div>
            <span class="jf-jobs__count"><span>142</span><span class="jf-jobs__count-label">jobs available</span></span>
          </header>
          <div class="jf-jobs-toolbar"><div class="jf-jobs-filters">
            <div class="jf-jobs-search">🔍 <input type="search" class="jf-jobs-search__input" placeholder="Search by job title…"></div>
            <div class="jf-jobs-filter jf-jobs-filter--select"><select class="jf-jobs-filter__select"><option>All sectors</option><option>IT / Software</option><option>Finance</option></select></div>
            <div class="jf-jobs-filter jf-jobs-filter--select"><select class="jf-jobs-filter__select"><option>All cities</option><option>Casablanca</option><option>Rabat</option></select></div>
          </div></div>
          <div id="job-list-container">__CARDS__</div>
        </div>
      </section>
    </div>
    """ + footer()
    body = body.replace("__CARDS__", '<div class="jf-jobs__grid">%s</div>' % "".join(job_cards()))
    return HTML_HEAD % (SHIM + CSS_NAV + CSS_INDEX, body)


def html_home_filtered():
    """Search results view (Sprint 6 advanced search)."""
    body = topbar("Jobs") + """
    <div class="jf-home">
      <section class="jf-hero" style="min-height:38vh;">
        <div class="jf-hero__bg" style="background:linear-gradient(135deg,rgba(15,23,42,.92),rgba(30,41,59,.78) 45%,rgba(79,70,229,.55));"></div>
        <div class="jf-hero__overlay"></div>
        <div class="container jf-hero__content" style="padding:5rem 1rem 3rem;">
          <span class="jf-hero__badge">Search results</span>
          <h1 class="jf-hero__title" style="font-size:2.5rem;">"developer" in Casablanca</h1>
          <p class="jf-hero__subtitle">3 jobs match your criteria.</p>
        </div>
      </section>
      <section id="jobs" class="jf-jobs">
        <div class="container">
          <header class="jf-jobs__header">
            <div><h2>Filtered results</h2><p>Search: "developer" · Sector: IT / Software · City: Casablanca</p></div>
            <span class="jf-jobs__count"><span>3</span><span class="jf-jobs__count-label">jobs found</span></span>
          </header>
          <div class="jf-jobs-toolbar"><div class="jf-jobs-filters">
            <div class="jf-jobs-search">🔍 <input type="search" class="jf-jobs-search__input" value="developer"></div>
            <div class="jf-jobs-filter jf-jobs-filter--select"><select class="jf-jobs-filter__select"><option>IT / Software</option></select></div>
            <div class="jf-jobs-filter jf-jobs-filter--select"><select class="jf-jobs-filter__select"><option>Casablanca</option></select></div>
          </div></div>
          <div id="job-list-container">__CARDS__</div>
          <nav class="jf-pagination">
            <p class="jf-pagination__info">Showing 3 of 3 — Page 1 of 1</p>
            <ul class="pagination"><li class="active"><span>1</span></li></ul>
          </nav>
        </div>
      </section>
    </div>
    """ + footer()
    body = body.replace("__CARDS__", '<div class="jf-jobs__grid">%s</div>' % "".join(job_cards()[:3]))
    return HTML_HEAD % (SHIM + CSS_NAV + CSS_INDEX, body)


def html_pagination():
    """Pagination view (Sprint 6 iface2) — multi-page nav."""
    cards = []
    for title, company, city, sector, ini in JOBS:
        cards.append(
            '<article class="jf-job-card"><div class="jf-job-card__avatar">%s</div>'
            '<div class="jf-job-card__body"><div class="jf-job-card__top">'
            '<h3 class="jf-job-card__title">%s</h3><span class="jf-job-card__badge">%s</span></div>'
            '<div class="jf-job-card__meta"><span>🏢 %s</span><span>📍 %s, Morocco</span></div></div>'
            '<div class="jf-job-card__action"><a class="jf-btn jf-btn--primary">View &amp; Apply</a></div></article>'
            % (ini, title, sector, company, city))
    pager = (
        '<nav class="jf-pagination"><p class="jf-pagination__info">'
        'Showing 6 of 142 — Page 3 of 24</p>'
        '<ul class="pagination">'
        '<li class="disabled"><span>«</span></li>'
        '<li><a>1</a></li><li><a>2</a></li><li class="active"><span>3</span></li>'
        '<li><a>4</a></li><li><a>5</a></li><li><a>»</a></li>'
        '</ul></nav>')
    body = topbar("Jobs") + (
        '<div class="jf-home"><section class="jf-jobs" style="padding-top:2.5rem;">'
        '<div class="container">'
        '<header class="jf-jobs__header"><div><h2>Latest opportunities</h2>'
        '<p>Hand-picked roles updated regularly from trusted recruiters.</p></div>'
        '<span class="jf-jobs__count"><span>142</span><span class="jf-jobs__count-label">jobs available</span></span></header>'
        '<div class="jf-jobs-toolbar"><div class="jf-jobs-filters">'
        '<div class="jf-jobs-search">🔍 <input type="search" class="jf-jobs-search__input" placeholder="Search by job title…"></div>'
        '<div class="jf-jobs-filter jf-jobs-filter--select"><select class="jf-jobs-filter__select"><option>All sectors</option></select></div>'
        '<div class="jf-jobs-filter jf-jobs-filter--select"><select class="jf-jobs-filter__select"><option>All cities</option></select></div>'
        '</div></div>'
        '<div id="job-list-container"><div class="jf-jobs__grid">%s</div>%s</div>'
        '</div></section></div>' % ("".join(cards), pager)) + footer()
    return HTML_HEAD % (SHIM + CSS_NAV + CSS_INDEX, body)


def html_job_detail():
    body = topbar("Jobs") + """
    <div class="jf-job-page">
      <section class="jf-job-hero">
        <div class="container">
          <nav class="jf-job-hero__breadcrumb"><a>Home</a><span>›</span><a>Jobs</a><span>›</span><span>Senior Frontend Developer</span></nav>
          <h1 class="jf-job-hero__title">Senior Frontend Developer</h1>
          <div class="jf-job-hero__meta">
            <span class="jf-job-hero__badge">IT / Software</span>
            <span>🏢 InnovaTech</span><span>📍 Casablanca, Morocco</span>
          </div>
        </div>
      </section>
      <section class="jf-job-body"><div class="container"><div class="jf-job-layout">
        <div class="jf-job-card-panel">
          <h2>Job description</h2>
          <div class="jf-job-description">
            <p><strong>About the role</strong></p>
            <p>We are looking for a Senior Frontend Developer to join our product team and build modern, responsive web applications used by thousands of recruiters and candidates.</p>
            <p><strong>Responsibilities</strong></p>
            <ul>
              <li>Build and maintain ASP.NET MVC Razor views with clean, accessible HTML and CSS.</li>
              <li>Collaborate with the backend team on the JF.DAL data access layer.</li>
              <li>Optimize the public job board for performance and SEO.</li>
            </ul>
            <p><strong>Requirements</strong></p>
            <ul><li>5+ years of frontend development experience.</li><li>Strong knowledge of HTML5, CSS3, JavaScript and Bootstrap.</li></ul>
          </div>
        </div>
        <aside class="jf-job-sidebar"><div class="jf-job-card-panel">
          <h3>Job overview</h3>
          <div class="jf-job-sidebar__item"><span class="jf-job-sidebar__icon">💼</span><div><div class="jf-job-sidebar__label">Position</div><div class="jf-job-sidebar__value">Senior Frontend Developer</div></div></div>
          <div class="jf-job-sidebar__item"><span class="jf-job-sidebar__icon">🏢</span><div><div class="jf-job-sidebar__label">Company</div><div class="jf-job-sidebar__value"><a>InnovaTech</a></div></div></div>
          <div class="jf-job-sidebar__item"><span class="jf-job-sidebar__icon">📍</span><div><div class="jf-job-sidebar__label">Location</div><div class="jf-job-sidebar__value">Casablanca, Morocco</div></div></div>
          <div class="jf-job-sidebar__item"><span class="jf-job-sidebar__icon">✉️</span><div><div class="jf-job-sidebar__label">Contact</div><div class="jf-job-sidebar__value"><a>careers@innovatech.ma</a></div></div></div>
          <a class="jf-btn jf-btn--primary">Apply for this job</a>
        </div></aside>
      </div></div></section>
    </div>
    """ + footer()
    return HTML_HEAD % (SHIM + CSS_NAV + CSS_JOB, body)


def html_apply():
    body = topbar("Jobs") + """
    <div class="jf-job-page">
      <section class="jf-job-hero">
        <div class="container">
          <nav class="jf-job-hero__breadcrumb"><a>Home</a><span>›</span><a>Senior Frontend Developer</a><span>›</span><span>Apply</span></nav>
          <h1 class="jf-job-hero__title">Apply for this position</h1>
          <p class="jf-job-hero__meta" style="margin:0;"><span>💼 Senior Frontend Developer</span></p>
        </div>
      </section>
      <section class="jf-job-body"><div class="container"><div class="jf-job-layout">
        <div class="jf-job-card-panel">
          <div class="jf-apply-summary"><h4>You're applying for</h4><p><strong>Senior Frontend Developer</strong> — fill in your details below and upload your CV to complete your application.</p></div>
          <form class="jf-apply-form">
            <div class="row">
              <div class="col-md-6"><div class="form-group"><label>Last name</label><input class="form-control" placeholder="Last name"></div></div>
              <div class="col-md-6"><div class="form-group"><label>First name</label><input class="form-control" placeholder="First name"></div></div>
            </div>
            <div class="form-group"><label>Mobile number</label><input class="form-control" placeholder="0612345678"></div>
            <div class="form-group"><label>Email address</label><input class="form-control" placeholder="you@email.com"></div>
            <div class="form-group"><label>CV / Resume</label>
              <div class="jf-upload"><input type="file" class="jf-upload__input">
                <div class="jf-upload__zone">
                  <div class="jf-upload__icon">☁️</div>
                  <p class="jf-upload__title">Drag &amp; drop your CV here</p>
                  <p class="jf-upload__hint">or <span class="jf-upload__browse">browse files</span></p>
                  <p class="jf-upload__formats">PDF, DOC, DOCX — max 10 MB</p>
                </div>
              </div>
            </div>
            <div class="form-group"><label>Cover message <span style="font-weight:400;color:#94a3b8;">(optional)</span></label><textarea class="form-control" placeholder="Tell the employer why you're a great fit for this role..."></textarea></div>
            <button type="button" class="jf-btn jf-btn--primary">📨 Submit application</button>
          </form>
        </div>
        <aside class="jf-job-sidebar"><div class="jf-job-card-panel">
          <h3>Before you apply</h3>
          <div class="jf-job-sidebar__item"><span class="jf-job-sidebar__icon">✅</span><div><div class="jf-job-sidebar__value" style="font-weight:500;color:#475569;line-height:1.6;">Make sure your CV is up to date and relevant to this role.</div></div></div>
          <div class="jf-job-sidebar__item"><span class="jf-job-sidebar__icon">🔒</span><div><div class="jf-job-sidebar__value" style="font-weight:500;color:#475569;line-height:1.6;">Your information is only shared with the employer for this application.</div></div></div>
          <a class="jf-btn jf-btn--ghost">Back to job details</a>
        </div></aside>
      </div></div></section>
    </div>
    """ + footer()
    return HTML_HEAD % (SHIM + CSS_NAV + CSS_JOB, body)


# ---------------------------------------------------------------------------
# Profil mockups (Sprint 1)
# ---------------------------------------------------------------------------

def html_register():
    auth_css = """
    .jf-auth { min-height:100vh; display:flex; align-items:center; justify-content:center;
      background:linear-gradient(135deg,#0f172a,#1e293b 50%,#312e81); padding:3rem 1rem; }
    .jf-auth__card { background:#fff; border-radius:24px; max-width:520px; width:100%;
      padding:2.5rem; box-shadow:0 30px 60px rgba(15,23,42,.4); }
    .jf-auth__brand { font-weight:800; font-size:1.4rem; text-align:center; margin-bottom:.4rem; color:#0f172a;}
    .jf-auth__brand b{color:#4f46e5;}
    .jf-auth__title { text-align:center; font-size:1.5rem; font-weight:800; margin:0 0 .25rem;}
    .jf-auth__sub { text-align:center; color:#64748b; margin:0 0 1.75rem; font-size:.95rem;}
    .jf-auth .form-group{margin-bottom:1.1rem;}
    .jf-auth label{display:block;font-size:.85rem;font-weight:600;color:#475569;margin-bottom:.4rem;}
    .jf-auth .form-control{width:100%;padding:.8rem 1rem;border:1px solid #e2e8f0;border-radius:12px;font-size:.95rem;}
    .jf-auth__btn{width:100%;padding:1rem;border:none;border-radius:12px;background:linear-gradient(135deg,#4f46e5,#6366f1);color:#fff;font-weight:700;font-size:1rem;cursor:pointer;box-shadow:0 10px 28px rgba(79,70,229,.35);}
    .jf-auth__alt{text-align:center;margin:1.25rem 0 0;color:#64748b;font-size:.9rem;}
    .jf-auth__alt a{color:#4f46e5;font-weight:700;}
    .jf-auth__check{display:flex;align-items:center;gap:.5rem;font-size:.85rem;color:#475569;margin-bottom:1.1rem;}
    .jf-auth__check input{width:16px;height:16px;}
    """
    body = """
    <div class="jf-auth"><div class="jf-auth__card">
      <div class="jf-auth__brand">Jobz<b>Factory</b></div>
      <h1 class="jf-auth__title">Create your account</h1>
      <p class="jf-auth__sub">Join JobzFactory and apply to top jobs in a few clicks.</p>
      <form>
        <div class="row"><div class="col-md-6"><div class="form-group"><label>Last name</label><input class="form-control" placeholder="Fisly"></div></div>
        <div class="col-md-6"><div class="form-group"><label>First name</label><input class="form-control" placeholder="Oussama"></div></div></div>
        <div class="form-group"><label>Email address</label><input class="form-control" placeholder="you@email.com"></div>
        <div class="form-group"><label>Password</label><input class="form-control" type="password" placeholder="••••••••"></div>
        <div class="form-group"><label>Confirm password</label><input class="form-control" type="password" placeholder="••••••••"></div>
        <label class="jf-auth__check"><input type="checkbox"> I accept the terms and conditions</label>
        <button class="jf-auth__btn" type="button">Create account</button>
        <p class="jf-auth__alt">Already have an account? <a>Log in</a></p>
      </form>
    </div></div>
    """
    return HTML_HEAD % (SHIM + auth_css, body)


def html_profile():
    prof_css = CSS_PROFIL + """
    .jf-pf-page{background:#f8fafc;min-height:100vh;padding:2rem 0;}
    .jf-pf-card{background:#fff;border:1px solid #e2e8f0;border-radius:18px;padding:1.75rem;margin-bottom:1.5rem;box-shadow:0 4px 20px rgba(15,23,42,.04);}
    .jf-pf-head{display:flex;align-items:center;gap:1.25rem;margin-bottom:1.5rem;}
    .jf-pf-avatar{width:72px;height:72px;border-radius:18px;background:linear-gradient(135deg,#4f46e5,#6366f1);color:#fff;font-size:1.8rem;font-weight:800;display:flex;align-items:center;justify-content:center;}
    .jf-pf-head h1{margin:0;font-size:1.5rem;}
    .jf-pf-head p{margin:.2rem 0 0;color:#64748b;}
    .jf-pf-grid{display:grid;grid-template-columns:1fr 1fr;gap:1.25rem;}
    .jf-pf-field{margin-bottom:1rem;}
    .jf-pf-field label{display:block;font-size:.78rem;font-weight:700;text-transform:uppercase;letter-spacing:.04em;color:#64748b;margin-bottom:.3rem;}
    .jf-pf-field .v{font-size:1rem;font-weight:600;color:#0f172a;}
    .jf-pf-section h2{font-size:1.1rem;margin:0 0 1rem;}
    .jf-pf-btn{padding:.7rem 1.25rem;border-radius:12px;border:none;background:linear-gradient(135deg,#4f46e5,#6366f1);color:#fff;font-weight:700;cursor:pointer;}
    .jf-pf-cv{display:flex;align-items:center;gap:1rem;padding:1rem 1.25rem;border:1px solid #c7d2fe;border-radius:14px;background:#eef2ff;margin-bottom:.85rem;}
    .jf-pf-cv__icon{width:44px;height:44px;border-radius:10px;background:#fff;color:#4f46e5;display:flex;align-items:center;justify-content:center;font-size:1.25rem;border:1px solid #c7d2fe;}
    .jf-pf-cv__name{font-weight:700;color:#0f172a;}
    .jf-pf-cv__meta{font-size:.8rem;color:#64748b;}
    """
    body = """
    <div class="jf-pf-page"><div class="container">
      <div class="jf-pf-card"><div class="jf-pf-head">
        <div class="jf-pf-avatar">OF</div>
        <div><h1>Oussama Fisly</h1><p>📍 Casablanca, Morocco · ✉️ oussama.fisly@email.com</p></div>
        <div style="margin-left:auto;"><button class="jf-pf-btn">Edit profile</button></div>
      </div>
      <div class="jf-pf-grid">
        <div class="jf-pf-field"><label>Title</label><div class="v">Full-Stack .NET Developer</div></div>
        <div class="jf-pf-field"><label>Sector</label><div class="v">IT / Software</div></div>
        <div class="jf-pf-field"><label>Mobile</label><div class="v">0612345678</div></div>
        <div class="jf-pf-field"><label>Date of birth</label><div class="v">14 / 06 / 2000</div></div>
        <div class="jf-pf-field"><label>Address</label><div class="v">12 Rue des Oliviers, Casablanca</div></div>
        <div class="jf-pf-field"><label>Account state</label><div class="v" style="color:#15803d;">Active</div></div>
      </div></div>
      <div class="jf-pf-card jf-pf-section">
        <h2>My CVs</h2>
        <div class="jf-pf-cv"><div class="jf-pf-cv__icon">📄</div><div><div class="jf-pf-cv__name">CV_Oussama_Fisly_2026.pdf</div><div class="jf-pf-cv__meta">Uploaded 12/05/2026 · 248 KB</div></div></div>
        <div class="jf-pf-cv"><div class="jf-pf-cv__icon">📄</div><div><div class="jf-pf-cv__name">CV_Alternative.docx</div><div class="jf-pf-cv__meta">Uploaded 03/04/2026 · 76 KB</div></div></div>
        <button class="jf-pf-btn">+ Upload new CV</button>
      </div>
    </div></div>
    """
    return HTML_HEAD % (SHIM + prof_css, body)


# ---------------------------------------------------------------------------
# Recruiter portal mockups
# ---------------------------------------------------------------------------

def rc_shell(active, content, fixed=False):
    nav = [
        ("Dashboard", "📊"), ("Offers", "📋"), ("Applications", "📨"),
        ("Profile", "🏢"), ("Logout", "⎋"),
    ]
    items = "".join(
        '<li><a class="%s"><i>%s</i> %s</a></li>' % (
            "active" if name == active else "", icon, name)
        for name, icon in nav)
    body_class = "jf-rc-layout-fixed" if fixed else ""
    return HTML_HEAD % (SHIM + CSS_RC, (
        '<body class="%s"><div class="jf-rc"><div class="jf-rc-app">'
        '<aside class="jf-rc-sidebar">'
        '<div class="jf-rc-brand"><span>JF</span> JobzFactory</div>'
        '<ul class="jf-rc-nav">%s</ul>'
        '<div class="jf-rc-sidebar__footer"><div class="jf-rc-user"><strong>Sara Bennani</strong>Recruiter · InnovaTech</div></div>'
        '</aside>'
        '<div class="jf-rc-main"><div class="jf-rc-content">%s</div>'
        '<div class="jf-rc-footer">© 2026 JobzFactory — Recruiter space</div>'
        '</div></div></div></body>' % (body_class, items, content)))


def html_recruiter_dashboard():
    content = """
    <div class="jf-rc-header"><div><h1>Welcome back, Sara</h1><p>Here's what's happening with your recruitment today.</p></div>
    <a class="jf-rc-btn jf-rc-btn--primary">➕ New offer</a></div>
    <div class="jf-rc-stat-grid">
      <div class="jf-rc-stat jf-rc-stat--indigo"><i class="jf-rc-stat__icon">💼</i><strong>12</strong><span>Total offers</span></div>
      <div class="jf-rc-stat jf-rc-stat--green"><i class="jf-rc-stat__icon">✅</i><strong>8</strong><span>Published</span></div>
      <div class="jf-rc-stat jf-rc-stat--amber"><i class="jf-rc-stat__icon">📝</i><strong>4</strong><span>Drafts</span></div>
      <div class="jf-rc-stat jf-rc-stat--teal"><i class="jf-rc-stat__icon">👥</i><strong>57</strong><span>Applications</span></div>
    </div>
    <div class="jf-rc-dashboard-grid">
      <div class="jf-rc-card"><div class="jf-rc-card__header"><h2>Recent offers</h2><a class="jf-rc-card__link">View all</a></div>
        <div class="jf-rc-table-wrap"><table class="jf-rc-table"><thead><tr><th>Title</th><th>City</th><th>Status</th><th>Apps</th></tr></thead><tbody>
          <tr><td><a>Senior Frontend Developer</a></td><td>Casablanca</td><td><span class="jf-rc-badge jf-rc-badge--live">Live</span></td><td>18</td></tr>
          <tr><td><a>Data Analyst</a></td><td>Rabat</td><td><span class="jf-rc-badge jf-rc-badge--live">Live</span></td><td>9</td></tr>
          <tr><td><a>DevOps Engineer</a></td><td>Marrakech</td><td><span class="jf-rc-badge jf-rc-badge--draft">Draft</span></td><td>0</td></tr>
          <tr><td><a>UX Designer</a></td><td>Casablanca</td><td><span class="jf-rc-badge jf-rc-badge--live">Live</span></td><td>12</td></tr>
        </tbody></table></div>
      </div>
      <div class="jf-rc-card"><div class="jf-rc-card__header"><h2>Recent applications</h2></div>
        <ul class="jf-rc-feed">
          <li class="jf-rc-feed__item"><div class="jf-rc-feed__avatar">O</div><div class="jf-rc-feed__content"><strong>Oussama Fisly</strong><p>Applied to <a>Senior Frontend Developer</a></p><span class="jf-rc-feed__time">12/06/2026 09:24</span></div><a class="jf-rc-btn jf-rc-btn--ghost" style="font-size:.8rem;padding:.45rem .75rem;">CV</a></li>
          <li class="jf-rc-feed__item"><div class="jf-rc-feed__avatar">L</div><div class="jf-rc-feed__content"><strong>Leila Mansouri</strong><p>Applied to <a>Data Analyst</a></p><span class="jf-rc-feed__time">11/06/2026 16:40</span></div><a class="jf-rc-btn jf-rc-btn--ghost" style="font-size:.8rem;padding:.45rem .75rem;">CV</a></li>
          <li class="jf-rc-feed__item"><div class="jf-rc-feed__avatar">Y</div><div class="jf-rc-feed__content"><strong>Youssef Alaoui</strong><p>Applied to <a>UX Designer</a></p><span class="jf-rc-feed__time">10/06/2026 11:05</span></div><a class="jf-rc-btn jf-rc-btn--ghost" style="font-size:.8rem;padding:.45rem .75rem;">CV</a></li>
        </ul>
      </div>
    </div>
    <div class="jf-rc-card jf-rc-quick-actions"><h2>Quick actions</h2>
      <div class="jf-rc-quick-actions__grid">
        <a class="jf-rc-quick-action"><i>➕</i><span>Create offer</span></a>
        <a class="jf-rc-quick-action"><i>📋</i><span>Manage offers</span></a>
        <a class="jf-rc-quick-action"><i>🌐</i><span>View job site</span></a>
      </div>
    </div>
    """
    return rc_shell("Dashboard", content)


def html_recruiter_offers():
    rows = [
        ("Senior Frontend Developer", "Casablanca", True, 18),
        ("Data Analyst", "Rabat", True, 9),
        ("DevOps Engineer", "Marrakech", False, 0),
        ("UX Designer", "Casablanca", True, 12),
        ("Accountant", "Tanger", True, 7),
        ("Marketing Manager", "Casablanca", False, 0),
    ]
    trs = ""
    for title, city, live, apps in rows:
        badge = ('<span class="jf-rc-badge jf-rc-badge--live">Live</span>' if live
                 else '<span class="jf-rc-badge jf-rc-badge--draft">Draft</span>')
        trs += ('<tr><td><a>%s</a></td><td>%s</td><td>%s</td><td>%d</td>'
                '<td><a class="jf-rc-btn jf-rc-btn--ghost" style="font-size:.75rem;padding:.35rem .6rem;">Edit</a></td></tr>'
                % (title, city, badge, apps))
    content = """
    <div class="jf-rc-offers-page">
      <div class="jf-rc-header jf-rc-header--compact"><div><h1>Job offers</h1><p>Manage your listings and publication status.</p></div>
      <a class="jf-rc-btn jf-rc-btn--primary">➕ New offer</a></div>
      <div class="jf-rc-card jf-rc-offers-toolbar">
        <div class="jf-rc-offers-filters jf-rc-offers-filters--primary">
          <div class="jf-rc-search-field">🔍 <input type="search" class="form-control" placeholder="Search by title…"></div>
          <div class="jf-rc-filter-select"><select class="form-control jf-rc-select"><option>All sectors</option></select></div>
          <div class="jf-rc-filter-select"><select class="form-control jf-rc-select"><option>All cities</option></select></div>
        </div>
        <div class="jf-rc-offers-filters jf-rc-offers-filters--status">
          <div class="jf-rc-filter-tabs">
            <button class="jf-rc-filter-tab is-active">All</button>
            <button class="jf-rc-filter-tab">Live</button>
            <button class="jf-rc-filter-tab">Draft</button>
          </div>
        </div>
      </div>
      <div class="jf-rc-card jf-rc-offers-list-card">
        <div id="offer-list-container"><div class="jf-rc-offer-results">
          <div class="jf-rc-table-wrap"><table class="jf-rc-table"><thead><tr><th>Title</th><th>City</th><th>Status</th><th>Apps</th><th>Actions</th></tr></thead><tbody>%s</tbody></table></div>
          <div class="jf-rc-pagination"><p class="jf-rc-pagination__info">Showing 6 of 12 offers</p></div>
        </div></div>
      </div>
    </div>
    """ % trs
    return rc_shell("Offers", content, fixed=True)


def html_recruiter_board():
    content = """
    <div class="jf-rc-header"><div><h1>Senior Frontend Developer</h1><p>Casablanca · IT / Software · Published · 18 applicants</p></div>
      <div style="display:flex;gap:.5rem;">
        <a class="jf-rc-btn jf-rc-btn--primary">✏️ Edit offer</a>
        <a class="jf-rc-btn jf-rc-btn--danger">🗑 Delete</a>
        <a class="jf-rc-btn jf-rc-btn--ghost">⬅ All offers</a>
      </div></div>
    <div class="jf-rc-card">
      <div class="jf-rc-tabs">
        <button class="jf-rc-tab is-active">Information</button>
        <button class="jf-rc-tab">History</button>
        <button class="jf-rc-tab">Candidates (18)</button>
      </div>
      <div class="jf-rc-tab-panels">
        <div class="jf-rc-tab-pane is-active"><div class="jf-rc-board-grid">
          <div><div class="jf-rc-description">
            <p><strong>About the role</strong></p>
            <p>We are looking for a Senior Frontend Developer to join our product team and build modern, responsive web applications used by thousands of recruiters and candidates.</p>
            <p><strong>Responsibilities</strong></p>
            <ul><li>Build and maintain ASP.NET MVC Razor views with clean, accessible HTML and CSS.</li><li>Collaborate with the backend team on the JF.DAL data access layer.</li><li>Optimize the public job board for performance and SEO.</li></ul>
          </div></div>
          <aside>
            <div class="jf-rc-sidebar-card"><h3>Status</h3><span class="jf-rc-badge jf-rc-badge--live">Live</span>
              <p style="margin:.75rem 0 0;color:#64748b;font-size:.9rem;">Visible on the public job site.</p>
              <a class="jf-rc-btn jf-rc-btn--ghost" style="width:100%;margin-top:1rem;">Unpublish</a>
            </div>
            <div class="jf-rc-sidebar-card" style="margin-top:1rem;"><h3>Details</h3>
              <dl class="jf-rc-meta-list"><dt>City</dt><dd>Casablanca</dd><dt>Sector</dt><dd>IT / Software</dd>
              <dt>Application email</dt><dd>careers@innovatech.ma</dd><dt>Public URL</dt><dd><a>View on job site</a></dd></dl>
            </div>
          </aside>
        </div></div>
      </div>
    </div>
    """
    return rc_shell("Offers", content)


def html_candidatures():
    cands = [
        ("Oussama Fisly", "oussama.fisly@email.com", "0612345678", "12/06/2026 09:24",
         "I have 5 years of frontend experience and I am very interested in this role."),
        ("Leila Mansouri", "leila.m@email.com", "0623456789", "11/06/2026 16:40",
         "No cover message submitted."),
        ("Youssef Alaoui", "y.alaoui@email.com", "0634567890", "10/06/2026 11:05",
         "Available immediately, passionate about accessible UIs."),
    ]
    cards = ""
    for name, email, gsm, date, msg in cands:
        ini = name[0]
        cards += (
            '<article class="jf-rc-candidate-card"><div class="jf-rc-candidate-card__header">'
            '<div><h3>%s</h3><p class="jf-rc-candidate-card__meta">Applied %s · %s · %s</p></div>'
            '<a class="jf-rc-btn jf-rc-btn--primary">View CV</a></div>'
            '<div class="jf-rc-candidate-card__body"><h4>Cover message</h4>'
            '<div class="jf-rc-candidate-card__message">%s</div></div>'
            '<div class="jf-rc-candidate-card__footer"><span>CV file: CV_%s.pdf</span></div></article>'
            % (name, date, email, gsm, msg, name.replace(" ", "_")))
    content = """
    <div class="jf-rc-header"><div><h1>Candidates — Senior Frontend Developer</h1><p>18 applicants received for this offer.</p></div>
      <a class="jf-rc-btn jf-rc-btn--ghost">⬅ Back to offer</a></div>
    <div class="jf-rc-card"><div class="jf-rc-candidate-list">%s</div></div>
    """ % cards
    return rc_shell("Applications", content)


# ---------------------------------------------------------------------------
# Administration portal mockups
# ---------------------------------------------------------------------------

def ad_shell(active, content):
    nav = [("Dashboard", "📊"), ("Offers", "📋"), ("Recruiters", "🏢"), ("Logout", "⎋")]
    items = "".join(
        '<li><a class="%s"><i>%s</i> %s</a></li>' % (
            "active" if name == active else "", icon, name)
        for name, icon in nav)
    return HTML_HEAD % (SHIM + CSS_AD, (
        '<body><div class="jf-ad"><div class="jf-ad-app">'
        '<aside class="jf-ad-sidebar">'
        '<div class="jf-ad-brand"><span>AD</span> JobzFactory</div>'
        '<ul class="jf-ad-nav">%s</ul>'
        '<div class="jf-ad-sidebar__footer"><div class="jf-ad-user"><strong>Administrator</strong>Back-office</div></div>'
        '</aside>'
        '<div class="jf-ad-main"><div class="jf-ad-content">%s</div>'
        '<div class="jf-ad-footer">© 2026 JobzFactory — Administration</div>'
        '</div></div></div></body>' % (items, content)))


def html_admin_dashboard():
    content = """
    <div class="jf-ad-header"><div><h1>Administration</h1><p>Manage job offers and recruiter accounts across the platform.</p></div></div>
    <div class="jf-ad-stat-grid">
      <div class="jf-ad-stat"><strong>142</strong><span>Total offers</span></div>
      <div class="jf-ad-stat"><strong>98</strong><span>Published</span></div>
      <div class="jf-ad-stat"><strong>34</strong><span>Recruiters</span></div>
    </div>
    <div class="jf-ad-card"><h2 style="margin:0 0 1rem;font-size:1.05rem;font-weight:700;">Quick actions</h2>
      <div class="jf-ad-actions">
        <a class="jf-ad-action"><i>📋</i><span>Manage offers</span></a>
        <a class="jf-ad-action"><i>➕</i><span>Create offer</span></a>
        <a class="jf-ad-action"><i>🏢</i><span>Manage recruiters</span></a>
        <a class="jf-ad-action"><i>👤</i><span>Add recruiter</span></a>
      </div>
    </div>
    """
    return ad_shell("Dashboard", content)


def html_admin_offers():
    rows = [
        ("Senior Frontend Developer", "InnovaTech", "Casablanca", True, False),
        ("Data Analyst", "DataCorp", "Rabat", True, False),
        ("DevOps Engineer", "CloudNine", "Marrakech", False, False),
        ("Secret Role", "Confidential", "Casablanca", True, True),
        ("UX Designer", "PixelLab", "Casablanca", True, False),
        ("Accountant", "FinTrust", "Tanger", False, False),
    ]
    trs = ""
    for title, rec, city, live, conf in rows:
        st = ('<span class="jf-ad-badge jf-ad-badge--live">Published</span>' if live
              else '<span class="jf-ad-badge jf-ad-badge--draft">Draft</span>')
        cf = ('<span class="jf-ad-badge jf-ad-badge--yes">Yes</span>' if conf
              else '<span class="jf-ad-badge jf-ad-badge--no">No</span>')
        trs += ('<tr><td><a>%s</a></td><td>%s</td><td>%s</td><td>%s</td><td>%s</td>'
                '<td><div class="jf-ad-row-actions">'
                '<a>✏️ Edit</a><a>👁 Details</a>'
                '<a class="jf-ad-row-actions__danger">🗑 Delete</a></div></td></tr>'
                % (title, rec, city, st, cf))
    content = """
    <div class="jf-ad-header"><div><h1>Job offers</h1><p>View and manage all listings in the database.</p></div>
      <a class="jf-ad-btn jf-ad-btn--primary">➕ New offer</a></div>
    <div class="jf-ad-card jf-ad-card--flush">
      <div class="jf-ad-table-wrap"><table class="jf-ad-table"><thead><tr>
        <th>Title</th><th>Recruiter</th><th>City</th><th>Status</th><th>Confidential</th><th>Actions</th>
      </tr></thead><tbody>%s</tbody></table></div>
    </div>
    """ % trs
    return ad_shell("Offers", content)


# ---------------------------------------------------------------------------
# Render via Playwright (Chromium) — reliable full-page screenshots
# ---------------------------------------------------------------------------

from playwright.sync_api import sync_playwright

_PW = None
_BROWSER = None
_CTX = None


def _pw_ctx():
    global _PW, _BROWSER, _CTX
    if _CTX is None:
        _PW = sync_playwright().start()
        _BROWSER = _PW.chromium.launch()
        _CTX = _BROWSER.new_context(viewport={"width": 1280, "height": 800},
                                    device_scale_factor=1)
    return _CTX


def render(html_str, out_name, width=1280, height=820):
    html_path = os.path.join(MOCK, out_name.replace(".png", ".html"))
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html_str)
    out_path = os.path.join(ASSETS, out_name)
    file_url = "file:///" + html_path.replace("\\", "/")
    ctx = _pw_ctx()
    page = ctx.new_page()
    try:
        page.set_viewport_size({"width": width, "height": height})
        page.goto(file_url, wait_until="networkidle")
        page.wait_for_timeout(400)
        page.screenshot(path=out_path, full_page=True)
    except Exception as e:
        print("  render error for %s: %s" % (out_name, e))
        return False
    finally:
        page.close()
    return os.path.exists(out_path)


def _pw_stop():
    global _PW, _BROWSER, _CTX
    try:
        if _CTX:
            _CTX.close()
        if _BROWSER:
            _BROWSER.close()
        if _PW:
            _PW.stop()
    except Exception:
        pass


def copy(src_name, dst_name):
    s = os.path.join(ASSETS, src_name)
    d = os.path.join(ASSETS, dst_name)
    shutil.copyfile(s, d)


def main():
    jobs = [
        ("register", html_register, 1100, 980, "fig_39.png"),
        ("profile", html_profile, 1280, 760, "fig_40.png"),
        ("home", html_home, 1280, 1400, "fig_41.png"),
        ("jobdetail", html_job_detail, 1280, 1100, "fig_42.png"),
        ("recruiter_offers", html_recruiter_offers, 1280, 820, "fig_43.png"),
        ("recruiter_board", html_recruiter_board, 1280, 860, "fig_44.png"),
        ("admin_offers", html_admin_offers, 1280, 720, "fig_45.png"),
        ("admin_dashboard", html_admin_dashboard, 1280, 720, "fig_46.png"),
        ("apply", html_apply, 1280, 1280, "fig_47.png"),
        ("candidatures", html_candidatures, 1280, 980, "fig_48.png"),
        ("search", html_home_filtered, 1280, 1100, "fig_49.png"),
        ("pagination", html_pagination, 1280, 1100, "fig_50.png"),
        ("recruiter_dashboard", html_recruiter_dashboard, 1280, 980, "fig_51.png"),
    ]
    generated = []
    for name, fn, w, h, out in jobs:
        ok = render(fn(), out, w, h)
        generated.append((out, ok))
        print("  %s  %s  (%dx%d)" % (out, "OK" if ok else "FAIL", w, h))

    # Aliases (appendix + sprint7 reuse)
    copy("fig_41.png", "fig_53.png")  # appendix public home desktop
    copy("fig_42.png", "fig_54.png")  # appendix offer detail (tablet)
    copy("fig_51.png", "fig_55.png")  # appendix recruiter dashboard
    copy("fig_45.png", "fig_56.png")  # appendix admin back-office
    copy("fig_47.png", "fig_57.png")  # appendix application form CV upload
    copy("fig_49.png", "fig_58.png")  # appendix advanced search results
    copy("fig_46.png", "fig_52.png")  # sprint7 iface2 = admin dashboard
    for extra in ["fig_53.png", "fig_54.png", "fig_55.png", "fig_56.png",
                  "fig_57.png", "fig_58.png", "fig_52.png"]:
        print("  %s  OK (alias)" % extra)

    ok_count = sum(1 for _, ok in generated if ok)
    print("Screenshots rendered: %d/%d primary" % (ok_count, len(generated)))
    _pw_stop()


if __name__ == "__main__":
    main()
