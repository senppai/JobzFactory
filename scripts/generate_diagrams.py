# -*- coding: utf-8 -*-
"""Generate UML / architecture diagrams for the JobzFactory report.

Outputs PNG files into scripts/report_assets/ named fig_NN.png where NN is the
figure number assigned by build_rapport.py (1..10 for the global diagrams and
11..52 for the 7 sprints: 4 diagrams each -> use case, class, seq1, seq2).

Uses Graphviz (dot) for use-case / class / architecture / deployment / package
diagrams and matplotlib for sequence diagrams (lifelines + horizontal messages).
"""
import os

os.environ["PATH"] = r"C:\Program Files\Graphviz\bin;" + os.environ.get("PATH", "")

from graphviz import Digraph
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle, FancyArrowPatch

ASSETS = os.path.join(os.path.dirname(__file__), "report_assets")
os.makedirs(ASSETS, exist_ok=True)

NAVY = "#1A4480"
INDIGO = "#4f46e5"
SLATE = "#0f172a"
SOFT = "#eef2ff"

# ---------------------------------------------------------------------------
# Graphviz helpers
# ---------------------------------------------------------------------------

def _base(name, rankdir="LR"):
    g = Digraph(name)
    g.attr(rankdir=rankdir, splines="spline", fontname="Helvetica",
           bgcolor="white", pad="0.3", nodesep="0.35", ranksep="0.9",
           dpi="170")
    g.attr("node", fontname="Helvetica", fontsize="11")
    g.attr("edge", fontname="Helvetica", fontsize="10", color="#475569")
    return g


def _save(g, filename):
    out = os.path.join(ASSETS, filename)
    # graphviz renders <name>.png; give it a temp name then move
    tmp = os.path.join(ASSETS, "_tmp_" + filename.replace(".png", ""))
    g.render(tmp, format="png", cleanup=True)
    produced = tmp + ".png"
    if os.path.exists(produced):
        os.replace(produced, out)
    return out


def _render_to(g, out_path):
    tmp = os.path.join(ASSETS, "_tmp_" + os.path.basename(out_path).replace(".png", ""))
    g.render(tmp, format="png", cleanup=True)
    produced = tmp + ".png"
    if os.path.exists(produced):
        os.replace(produced, out_path)
    return out_path


def _stitch_horizontal(paths, out_path, gap=24, bg="white"):
    from PIL import Image
    imgs = [Image.open(p).convert("RGB") for p in paths]
    h = max(im.height for im in imgs) + 40
    total_w = sum(im.width for im in imgs) + gap * (len(imgs) - 1) + 40
    canvas = Image.new("RGB", (total_w, h), bg)
    x = 20
    for im in imgs:
        canvas.paste(im, (x, 20))
        x += im.width + gap
    canvas.save(out_path)
    return out_path


def actor_node(g, name, label=None):
    g.node(name, label or name, shape="none", image="", fontcolor=NAVY,
           fontsize="12", fontname="Helvetica-Bold")


def usecase(g, name, label):
    g.node(name, label, shape="ellipse", style="filled", fillcolor=SOFT,
           color=NAVY, fontcolor=SLATE, width="2.1", height="0.7", margin="0.08")


def usecase_diagram(filename, title, actors, cases, links, system_label,
                    include=None, extend=None):
    """actors: list of (id,label). cases: list of (id,label). links: list of (actor_id, case_id).
    include/extend: list of (from_case, to_case) with dashed arrow."""
    g = _base("uc_" + filename.replace(".png", ""), rankdir="LR")
    with g.subgraph(name="cluster_sys") as c:
        c.attr(label=system_label, labelloc="t", style="rounded,dashed",
               color=NAVY, fontcolor=NAVY, fontname="Helvetica-Bold",
               fontsize="13", margin="18")
        for cid, lab in cases:
            usecase(c, cid, lab)
    for aid, lab in actors:
        actor_node(g, aid, lab)
    for aid, cid in links:
        g.edge(aid, cid, arrowhead="none")
    if include:
        for a, b in include:
            g.edge(a, b, style="dashed", arrowhead="open", label="&lt;&lt;include&gt;&gt;",
                   fontcolor=INDIGO, color=INDIGO)
    if extend:
        for a, b in extend:
            g.edge(a, b, style="dashed", arrowhead="open", label="&lt;&lt;extend&gt;&gt;",
                   fontcolor="#a855f7", color="#a855f7")
    g.attr(label=title, labelloc="t", fontname="Helvetica-Bold", fontsize="14",
           fontcolor=NAVY)
    _save(g, filename)


def usecase_diagram_two(filename, title, actors, clusters, system_label,
                        include=None, extend=None):
    """Two side-by-side system clusters (rendered separately then stitched)
    to keep the diagram landscape.
    clusters: list of (label, [case tuples], [(actor_id, case_id)])."""
    from PIL import Image
    parts = []
    for idx, (label, cases, links) in enumerate(clusters):
        g = _base("uc2_%d_%s" % (idx, filename.replace(".png", "")), rankdir="LR")
        g.attr(ranksep="1.0", nodesep="0.3")
        with g.subgraph(name="cluster_c%d" % idx) as c:
            c.attr(label=label, labelloc="t", style="rounded,dashed",
                   color=NAVY, fontcolor=NAVY, fontname="Helvetica-Bold",
                   fontsize="12", margin="14")
            for cid, lab in cases:
                usecase(c, cid, lab)
        cluster_case_ids = set(c[0] for c in cases)
        # actors relevant to this cluster
        rel_actors = [a for a in actors
                      if any(l[0] == a[0] and l[1] in cluster_case_ids for l in links)]
        for aid, lab in rel_actors:
            actor_node(g, aid, lab)
        for aid, cid in links:
            g.edge(aid, cid, arrowhead="none")
        p = os.path.join(ASSETS, "_part_%d_%s" % (idx, filename))
        _render_to(g, p)
        parts.append(p)
    out = os.path.join(ASSETS, filename)
    _stitch_horizontal(parts, out)
    # add title banner
    try:
        from PIL import Image, ImageDraw, ImageFont
        im = Image.open(out).convert("RGB")
        banner = Image.new("RGB", (im.width, 60), "white")
        d = ImageDraw.Draw(banner)
        try:
            font = ImageFont.truetype("arial.ttf", 30)
        except Exception:
            font = ImageFont.load_default()
        d.text((im.width // 2, 30), title, fill=NAVY, font=font, anchor="mm")
        combined = Image.new("RGB", (im.width, im.height + 60), "white")
        combined.paste(banner, (0, 0))
        combined.paste(im, (0, 60))
        combined.save(out)
    except Exception:
        pass
    for p in parts:
        try:
            os.remove(p)
        except OSError:
            pass
    return out


def class_node(g, name, attrs, methods, stereo=None):
    rows = ""
    if stereo:
        rows += '<TR><TD BGCOLOR="white"><FONT POINT-SIZE="9" COLOR="#7c3aed">«%s»</FONT></TD></TR>' % stereo
    rows += '<TR><TD BGCOLOR="%s"><FONT COLOR="white"><B>%s</B></FONT></TD></TR>' % (NAVY, name)
    attr_html = "<BR ALIGN=\"LEFT\"/>".join(
        "&#160;" + a for a in attrs) + "<BR ALIGN=\"LEFT\"/>"
    meth_html = "<BR ALIGN=\"LEFT\"/>".join(
        "&#160;" + m for m in methods) + "<BR ALIGN=\"LEFT\"/>"
    rows += '<TR><TD BALIGN="LEFT" ALIGN="LEFT">%s</TD></TR>' % attr_html
    rows += '<TR><TD BALIGN="LEFT" ALIGN="LEFT">%s</TD></TR>' % meth_html
    html = ('<<TABLE BORDER="1" CELLBORDER="0" CELLSPACING="0" '
            'CELLPADDING="4" BGCOLOR="white" COLOR="%s">%s</TABLE>>' % (NAVY, rows))
    g.node(name, html, shape="none")


def class_diagram(filename, title, classes, relations):
    """classes: list of dict(name, attrs[], methods[], stereo). relations: list of (a,b, headlabel, taillabel, style, arrow)."""
    g = _base("cls_" + filename.replace(".png", ""), rankdir="BT")
    g.attr(ranksep="1.1", nodesep="0.5")
    for c in classes:
        class_node(g, c["name"], c.get("attrs", []), c.get("methods", []),
                   c.get("stereo"))
    for r in relations:
        a, b = r[0], r[1]
        head = r[5] if len(r) > 5 else "none"
        style = r[4] if len(r) > 4 else "solid"
        g.edge(a, b, arrowhead=head, arrowtail="none", dir="both" if head != "none" else "none",
               headlabel=r[2] if len(r) > 2 else "", taillabel=r[3] if len(r) > 3 else "",
               labeldistance="1.4", labelangle="25", style=style, color=SLATE,
               fontcolor=NAVY, fontname="Helvetica", fontsize="10")
    g.attr(label=title, labelloc="t", fontname="Helvetica-Bold", fontsize="14",
           fontcolor=NAVY)
    _save(g, filename)


def arch_diagram(filename, title, layers, flow):
    """layers: list of (layer_name, [components]). flow: list of (a,b,label)."""
    g = _base("arch_" + filename.replace(".png", ""), rankdir="TB")
    g.attr(ranksep="1.0", nodesep="0.5")
    for i, (lname, comps) in enumerate(layers):
        with g.subgraph(name="cluster_l%d" % i) as c:
            c.attr(label=lname, style="rounded,filled", fillcolor=SOFT,
                   color=NAVY, fontcolor=NAVY, fontname="Helvetica-Bold",
                   fontsize="12", margin="12")
            for comp in comps:
                c.node(comp, comp, shape="box", style="filled,rounded",
                       fillcolor="white", color=NAVY, fontcolor=SLATE)
    for a, b, lab in flow:
        g.edge(a, b, label=lab, fontcolor=INDIGO, color=INDIGO)
    g.attr(label=title, labelloc="t", fontname="Helvetica-Bold", fontsize="14",
           fontcolor=NAVY)
    _save(g, filename)


def package_diagram(filename, title, packages, deps):
    g = _base("pkg_" + filename.replace(".png", ""), rankdir="LR")
    g.attr(ranksep="1.2", nodesep="0.6")
    for pid, label, stereo in packages:
        html = ('<<TABLE BORDER="1" CELLBORDER="0" CELLSPACING="0" CELLPADDING="6" '
                'BGCOLOR="white" COLOR="%s">'
                '<TR><TD BGCOLOR="%s"><FONT COLOR="white"><B>«%s» %s</B></FONT></TD></TR>'
                '</TABLE>>' % (NAVY, NAVY, stereo, label))
        g.node(pid, html, shape="none")
    for a, b in deps:
        g.edge(a, b, color=SLATE, arrowhead="vee")
    g.attr(label=title, labelloc="t", fontname="Helvetica-Bold", fontsize="14",
           fontcolor=NAVY)
    _save(g, filename)


def deployment_diagram(filename, title, nodes, links):
    g = _base("dep_" + filename.replace(".png", ""), rankdir="LR")
    g.attr(ranksep="1.3", nodesep="0.7")
    for nid, label, artifacts in nodes:
        html = ('<<TABLE BORDER="1" CELLBORDER="1" CELLSPACING="0" CELLPADDING="6" '
                'BGCOLOR="white" COLOR="%s" STYLE="ROUNDED">'
                '<TR><TD BGCOLOR="%s"><FONT COLOR="white"><B>%s</B></FONT></TD></TR>'
                '%s</TABLE>>' % (
                    NAVY, NAVY, label,
                    "".join('<TR><TD>«artifact» %s</TD></TR>' % a for a in artifacts)))
        g.node(nid, html, shape="none")
    for a, b, lab in links:
        g.edge(a, b, label=lab, fontcolor=INDIGO, color=INDIGO, dir="both")
    g.attr(label=title, labelloc="t", fontname="Helvetica-Bold", fontsize="14",
           fontcolor=NAVY)
    _save(g, filename)


# ---------------------------------------------------------------------------
# matplotlib sequence diagrams
# ---------------------------------------------------------------------------

def sequence_diagram(filename, title, actors, messages):
    """actors: list of names (left to right). messages: list of (from_idx, to_idx, label, style).
    style in {'sync','reply','self'}."""
    n = len(actors)
    fig_w = max(9.0, 2.0 * n + 1.5)
    height = max(4.5, 0.85 * len(messages) + 2.2)
    fig, ax = plt.subplots(figsize=(fig_w, height), dpi=170)
    ax.set_xlim(0, 2 * n + 1)
    ax.set_ylim(0, height)
    ax.axis("off")

    xs = [1 + 2 * i for i in range(n)]
    top = height - 0.6
    for x, name in zip(xs, actors):
        ax.add_patch(Rectangle((x - 1.0, top), 2.0, 0.55, facecolor=NAVY,
                               edgecolor=NAVY, zorder=5))
        ax.text(x, top + 0.27, name, ha="center", va="center", color="white",
                fontsize=10, fontweight="bold", zorder=6)
        ax.plot([x, x], [top, 0.4], linestyle="--", color="#94a3b8", lw=1, zorder=1)

    y = top - 0.5
    step = 0.8
    for m in messages:
        fi, ti, lab = m[0], m[1], m[2]
        style = m[3] if len(m) > 3 else "sync"
        y -= step
        if style == "self":
            xa = xs[fi]
            ax.add_patch(FancyArrowPatch((xa, y), (xa + 0.7, y - 0.18),
                                         arrowstyle="-|>", mutation_scale=12,
                                         color=SLATE, lw=1.4,
                                         connectionstyle="arc3,rad=-1.2"))
            ax.add_patch(FancyArrowPatch((xa + 0.7, y - 0.18), (xa, y - 0.36),
                                         arrowstyle="-|>", mutation_scale=12,
                                         color=SLATE, lw=1.4))
            ax.text(xa + 0.85, y - 0.05, lab, ha="left", va="center",
                    fontsize=8.5, color=SLATE)
            y -= 0.3
            continue
        xa, xb = xs[fi], xs[ti]
        ls = "--" if style == "reply" else "-"
        col = "#7c3aed" if style == "reply" else SLATE
        ax.add_patch(FancyArrowPatch((xa, y), (xb, y), arrowstyle="-|>",
                                     mutation_scale=14, color=col, lw=1.5,
                                     linestyle=ls))
        ax.text((xa + xb) / 2, y + 0.16, lab, ha="center", va="bottom",
                fontsize=8.5, color=col, fontweight="bold",
                bbox=dict(boxstyle="round,pad=0.15", fc="white", ec="none"))
        # activation box on target
        ax.add_patch(Rectangle((xb - 0.12, y - 0.18), 0.24, 0.32,
                               facecolor="#c7d2fe", edgecolor=NAVY, lw=0.6,
                               zorder=4))

    ax.text((2 * n + 1) / 2, height - 0.05, title, ha="center", va="top",
            fontsize=13, fontweight="bold", color=NAVY)
    fig.tight_layout()
    out = os.path.join(ASSETS, filename)
    fig.savefig(out, bbox_inches="tight", facecolor="white")
    plt.close(fig)
    return out


# ---------------------------------------------------------------------------
# Content definitions
# ---------------------------------------------------------------------------

GLOBAL_ACTORS = [
    ("Visitor", "Visitor"),
    ("Candidate", "Candidate"),
    ("Recruiter", "Recruiter"),
    ("Admin", "Administrator"),
]

GLOBAL_CASES = [
    ("uc_search", "Search / browse offers"),
    ("uc_view", "View offer details"),
    ("uc_register", "Register"),
    ("uc_activate", "Activate account"),
    ("uc_login", "Log in / log out"),
    ("uc_profile", "Manage profile"),
    ("uc_cv", "Manage CV files"),
    ("uc_apply", "Apply to an offer"),
    ("uc_create_offre", "Create / edit offer"),
    ("uc_publish", "Publish / unpublish offer"),
    ("uc_manage_apps", "Manage candidatures"),
    ("uc_dash", "View dashboard"),
    ("uc_moder_offre", "Moderate offers"),
    ("uc_moder_rec", "Validate recruiters"),
    ("uc_manage_rec", "Manage recruiters"),
    ("uc_search_adv", "Advanced search & filter"),
    ("uc_stats", "View statistics"),
    ("uc_history", "View offer history"),
    ("uc_sectors", "Manage reference data"),
    ("uc_reset", "Reset password"),
]

GLOBAL_LINKS = [
    ("Visitor", "uc_search"), ("Visitor", "uc_view"), ("Visitor", "uc_register"),
    ("Candidate", "uc_login"), ("Candidate", "uc_profile"), ("Candidate", "uc_cv"),
    ("Candidate", "uc_apply"), ("Candidate", "uc_search"), ("Candidate", "uc_view"),
    ("Candidate", "uc_reset"),
    ("Recruiter", "uc_login"), ("Recruiter", "uc_create_offre"),
    ("Recruiter", "uc_publish"), ("Recruiter", "uc_manage_apps"),
    ("Recruiter", "uc_dash"), ("Recruiter", "uc_history"),
    ("Admin", "uc_login"), ("Admin", "uc_moder_offre"), ("Admin", "uc_moder_rec"),
    ("Admin", "uc_manage_rec"), ("Admin", "uc_stats"), ("Admin", "uc_sectors"),
]
GLOBAL_INCLUDE = [("uc_apply", "uc_cv")]
GLOBAL_EXTEND = [("uc_moder_offre", "uc_publish")]


def global_class_classes():
    C = []
    C.append({"name": "Profil", "stereo": "candidate",
              "attrs": ["+ id : int", "+ nom : string", "+ prenom : string",
                        "+ adresseMail : string", "+ motPasse : string",
                        "+ dateNaissance : date", "+ titre : string",
                        "+ _idVille : int", "+ _idSecteur : int"],
              "methods": ["+ register()", "+ activate()", "+ login()",
                          "+ updateProfile()"]})
    C.append({"name": "Recruteur", "stereo": "company",
              "attrs": ["+ id : int", "+ nomRecruteur : string",
                        "+ siteweb : string", "+ adresseMail : string",
                        "+ description : string", "+ url : string"],
              "methods": ["+ create()", "+ update()"]})
    C.append({"name": "Recruteur_Contact", "stereo": "recruiter account",
              "attrs": ["+ id : int", "+ nom : string", "+ prenom : string",
                        "+ adresseMail : string", "+ motPasse : string",
                        "+ _idRecruteur : int"],
              "methods": ["+ login()", "+ logout()"]})
    C.append({"name": "Offre", "stereo": "offer",
              "attrs": ["+ id : int", "+ titreOffre : string",
                        "+ message : text", "+ isPublie : bool",
                        "+ isConfidentiel : bool", "+ url : string",
                        "+ _idRecruteur : int", "+ _idVille : int",
                        "+ _idSecteurActivite : int"],
              "methods": ["+ publish()", "+ disable()", "+ apply()"]})
    C.append({"name": "Offre_Postuler", "stereo": "application",
              "attrs": ["+ id : int", "+ _idOffre : int", "+ _idProfil : int",
                        "+ DatePostuler : datetime", "+ nomCV : string",
                        "+ message : text"],
              "methods": ["+ submit()", "+ delete()"]})
    C.append({"name": "Profil_CV", "stereo": "CV file",
              "attrs": ["+ id : int", "+ _idProfil : int",
                        "+ nomCV : string", "+ extantion : string",
                        "+ DateCreation : datetime"],
              "methods": ["+ upload()", "+ remove()"]})
    C.append({"name": "SecteurActivite", "stereo": "reference",
              "attrs": ["+ id : int", "+ SecteurActivite : string"],
              "methods": []})
    C.append({"name": "Ville", "stereo": "reference",
              "attrs": ["+ id : int", "+ ville : string", "+ _idPays : int"],
              "methods": []})
    C.append({"name": "Pays", "stereo": "reference",
              "attrs": ["+ id : int", "+ pays : string"], "methods": []})
    C.append({"name": "ActionOffre", "stereo": "moderation log",
              "attrs": ["+ id : int", "+ action : string"], "methods": []})
    C.append({"name": "Offre_Historique", "stereo": "history",
              "attrs": ["+ id : int", "+ _idOffre : int",
                        "+ _idActionOffre : int", "+ dateAction : datetime",
                        "+ commentaire : string"], "methods": []})
    return C


GLOBAL_CLASS_REL = [
    ("Profil", "Profil_CV", "1", "0..*", "solid", "none"),
    ("Profil", "Offre_Postuler", "1", "0..*", "solid", "none"),
    ("Recruteur", "Recruteur_Contact", "1", "0..*", "solid", "none"),
    ("Recruteur", "Offre", "1", "0..*", "solid", "none"),
    ("Offre", "Offre_Postuler", "1", "0..*", "solid", "none"),
    ("Offre", "Offre_Historique", "1", "0..*", "solid", "none"),
    ("Offre", "SecteurActivite", "0..*", "1", "solid", "none"),
    ("Offre", "Ville", "0..*", "1", "solid", "none"),
    ("Ville", "Pays", "0..*", "1", "solid", "none"),
    ("Offre_Historique", "ActionOffre", "0..*", "1", "solid", "none"),
    ("Profil", "Ville", "0..*", "1", "solid", "none"),
    ("Profil", "SecteurActivite", "0..*", "1", "solid", "none"),
]


# ---------------------------------------------------------------------------
# Sprint definitions
# ---------------------------------------------------------------------------

SPRINTS = [
    {
        "title": "Sprint 1: Authentication & candidate profile",
        "actors": [("Visitor", "Visitor"), ("Candidate", "Candidate"),
                   ("Admin", "Administrator")],
        "cases": [("uc_reg", "Register as candidate"), ("uc_act", "Activate account"),
                  ("uc_log", "Log in"), ("uc_out", "Log out"),
                  ("uc_prof", "Fill in profile"), ("uc_pwd", "Change password"),
                  ("uc_reset", "Reset password")],
        "links": [("Visitor", "uc_reg"), ("Candidate", "uc_act"),
                  ("Candidate", "uc_log"), ("Candidate", "uc_out"),
                  ("Candidate", "uc_prof"), ("Candidate", "uc_pwd"),
                  ("Candidate", "uc_reset")],
        "include": [("uc_reg", "uc_act")],
        "extend": [],
        "classes": [
            {"name": "Profil", "attrs": ["+ id", "+ nom", "+ prenom",
              "+ adresseMail", "+ motPasse", "+ dateCreation"], "methods": ["+ register()", "+ activate()", "+ authenticate()"]},
            {"name": "EmailService", "attrs": ["+ smtpHost", "+ fromAddress"], "methods": ["+ sendActivation()"]},
            {"name": "Session", "attrs": ["+ userId", "+ role"], "methods": ["+ setAuth()", "+ clear()"]},
        ],
        "rel": [("Profil", "EmailService", "1", "0..*", "dashed", "none"),
                ("Profil", "Session", "1", "0..*", "dashed", "none")],
        "seq1": ("Register & activate account",
                 ["Visitor", "SignUpController", "JF.DAL", "SQL Server", "EmailService"],
                 [(0,1,"register(nom,prenom,email,pwd)"), (1,2,"CreateProfil()"),
                  (2,3,"INSERT Profil (pending)"), (3,2,"id"), (2,1,"id"),
                  (1,4,"sendActivationEmail(id)"), (4,0,"activation link"),
                  (0,1,"activate(token)"), (1,2,"ActivateProfil()"),
                  (2,3,"UPDATE Profil SET state='Active'"), (3,2,"ok"), (2,1,"ok"), (1,0,"account activated")]),
        "seq2": ("Log in & access protected area",
                 ["Candidate", "LoginController", "JF.DAL", "SQL Server", "Session"],
                 [(0,1,"login(email,pwd)"), (1,2,"Authenticate()"),
                  (2,3,"SELECT Profil WHERE email & hash"), (3,2,"profil"),
                  (2,1,"profil"), (1,4,"setAuth(userId,role)"), (1,0,"redirect to dashboard"),
                  (0,1,"access profile page"), (1,4,"getAuth()"), (4,1,"userId"),
                  (1,2,"GetProfil(id)"), (2,3,"SELECT Profil"), (3,2,"profil"),
                  (2,1,"profil"), (1,0,"render profile")]),
        "cls_title": "Sprint 1 class diagram - Authentication",
        "uc_title": "Sprint 1 use case diagram - Authentication",
    },
    {
        "title": "Sprint 2: Public offer management",
        "actors": [("Visitor", "Visitor"), ("Candidate", "Candidate")],
        "cases": [("uc_browse", "Browse offers"), ("uc_search", "Search by title"),
                  ("uc_filter", "Filter by sector/city"), ("uc_detail", "View offer details"),
                  ("uc_company", "View recruiter profile")],
        "links": [("Visitor", "uc_browse"), ("Visitor", "uc_search"),
                  ("Visitor", "uc_filter"), ("Visitor", "uc_detail"),
                  ("Candidate", "uc_detail"), ("Candidate", "uc_company")],
        "include": [], "extend": [("uc_filter", "uc_search")],
        "classes": [
            {"name": "Offre", "attrs": ["+ id", "+ titreOffre", "+ message",
              "+ isPublie", "+ url", "+ _idRecruteur", "+ _idVille"], "methods": ["+ list()", "+ detail()"]},
            {"name": "Recruteur", "attrs": ["+ id", "+ nomRecruteur", "+ url"], "methods": []},
            {"name": "Ville", "attrs": ["+ id", "+ ville"], "methods": []},
            {"name": "SecteurActivite", "attrs": ["+ id", "+ SecteurActivite"], "methods": []},
        ],
        "rel": [("Offre", "Recruteur", "0..*", "1", "solid", "none"),
                ("Offre", "Ville", "0..*", "1", "solid", "none"),
                ("Offre", "SecteurActivite", "0..*", "1", "solid", "none")],
        "seq1": ("Browse & filter published offers",
                 ["Visitor", "DefaultController", "JF.DAL", "SQL Server"],
                 [(0,1,"Index(page,search,city,sector)"), (1,2,"ListOffers(filters)"),
                  (2,3,"SELECT Offre WHERE isPublie=1"), (3,2,"PagedList<Offre>"),
                  (2,1,"page"), (1,0,"render job list")]),
        "seq2": ("View offer detail page",
                 ["Candidate", "JobController", "JF.DAL", "SQL Server"],
                 [(0,1,"Detail(url)"), (1,2,"GetOffreByUrl(url)"),
                  (2,3,"SELECT Offre JOIN Recruteur,Ville"), (3,2,"offre"),
                  (2,1,"offre"), (1,0,"render detail page")]),
        "cls_title": "Sprint 2 class diagram - Public offers",
        "uc_title": "Sprint 2 use case diagram - Public offers",
    },
    {
        "title": "Sprint 3: Recruiter space",
        "actors": [("Recruiter", "Recruiter")],
        "cases": [("uc_create", "Create offer"), ("uc_edit", "Edit offer"),
                  ("uc_delete", "Delete offer"), ("uc_pub", "Publish / unpublish"),
                  ("uc_board", "View offer board"), ("uc_cand", "View candidatures")],
        "links": [("Recruiter", "uc_create"), ("Recruiter", "uc_edit"),
                  ("Recruiter", "uc_delete"), ("Recruiter", "uc_pub"),
                  ("Recruiter", "uc_board"), ("Recruiter", "uc_cand")],
        "include": [], "extend": [("uc_pub", "uc_create")],
        "classes": [
            {"name": "AnnonceController", "attrs": ["+ _dal"], "methods": ["+ Create()", "+ Edit()", "+ Delete()", "+ Board()"]},
            {"name": "Offre", "attrs": ["+ id", "+ titreOffre", "+ isPublie", "+ _idRecruteur"], "methods": ["+ save()", "+ publish()"]},
            {"name": "Offre_Historique", "attrs": ["+ id", "+ _idOffre", "+ dateAction"], "methods": ["+ log()"]},
            {"name": "Recruteur_Contact", "attrs": ["+ id", "+ _idRecruteur"], "methods": []},
        ],
        "rel": [("AnnonceController", "Offre", "1", "0..*", "dashed", "none"),
                ("Offre", "Offre_Historique", "1", "0..*", "solid", "none"),
                ("Recruteur_Contact", "Offre", "1", "0..*", "solid", "none")],
        "seq1": ("Create & publish an offer",
                 ["Recruiter", "AnnonceController", "JF.DAL", "SQL Server"],
                 [(0,1,"Create(offre)"), (1,2,"InsertOffre()"),
                  (2,3,"INSERT Offre (draft)"), (3,2,"id"), (2,1,"id"),
                  (1,0,"redirect to board"), (0,1,"Publish(id)"),
                  (1,2,"PublishOffre(id)"), (2,3,"UPDATE Offre SET isPublie=1"),
                  (2,3,"INSERT Offre_Historique"), (3,2,"ok"), (2,1,"ok"), (1,0,"offer live")]),
        "seq2": ("View offer board & candidates",
                 ["Recruiter", "AnnonceController", "JF.DAL", "SQL Server"],
                 [(0,1,"Board(id)"), (1,2,"GetOffreWithApps(id)"),
                  (2,3,"SELECT Offre + Offre_Postuler"), (3,2,"offre"),
                  (2,1,"offre"), (1,0,"render board"),
                  (0,1,"tab=Candidates"), (1,2,"ListApplications(id)"),
                  (2,3,"SELECT Offre_Postuler JOIN Profil"), (3,2,"apps"),
                  (2,1,"apps"), (1,0,"render candidates")]),
        "cls_title": "Sprint 3 class diagram - Recruiter space",
        "uc_title": "Sprint 3 use case diagram - Recruiter space",
    },
    {
        "title": "Sprint 4: Administration back-office",
        "actors": [("Admin", "Administrator")],
        "cases": [("uc_moder", "Moderate offers"), ("uc_valid", "Validate / reject offer"),
                  ("uc_del", "Delete offer"), ("uc_rec", "Manage recruiters"),
                  ("uc_addrec", "Add recruiter"), ("uc_log", "View moderation log")],
        "links": [("Admin", "uc_moder"), ("Admin", "uc_valid"), ("Admin", "uc_del"),
                  ("Admin", "uc_rec"), ("Admin", "uc_addrec"), ("Admin", "uc_log")],
        "include": [("uc_valid", "uc_log")], "extend": [],
        "classes": [
            {"name": "OffreController", "attrs": [], "methods": ["+ Index()", "+ Validate()", "+ Reject()", "+ Delete()"]},
            {"name": "RecruteurController", "attrs": [], "methods": ["+ Index()", "+ Create()", "+ Delete()"]},
            {"name": "Offre", "attrs": ["+ id", "+ isPublie"], "methods": []},
            {"name": "ActionOffre", "attrs": ["+ id", "+ action"], "methods": []},
            {"name": "Recruteur", "attrs": ["+ id", "+ nomRecruteur"], "methods": []},
        ],
        "rel": [("OffreController", "Offre", "1", "0..*", "dashed", "none"),
                ("Offre", "ActionOffre", "1", "0..*", "dashed", "none"),
                ("RecruteurController", "Recruteur", "1", "0..*", "dashed", "none")],
        "seq1": ("Validate / reject an offer",
                 ["Admin", "OffreController", "JF.DAL", "SQL Server"],
                 [(0,1,"Validate(id)"), (1,2,"PublishOffre(id)"),
                  (2,3,"UPDATE Offre SET isPublie=1"), (2,3,"INSERT Offre_Historique"),
                  (3,2,"ok"), (2,1,"ok"), (1,0,"offer published"),
                  (0,1,"Reject(id)"), (1,2,"RejectOffre(id)"),
                  (2,3,"UPDATE Offre SET isPublie=0"), (2,3,"INSERT Offre_Historique"),
                  (3,2,"ok"), (2,1,"ok"), (1,0,"offer rejected")]),
        "seq2": ("Add a recruiter account",
                 ["Admin", "RecruteurController", "JF.DAL", "SQL Server"],
                 [(0,1,"Create(recruteur)"), (1,2,"InsertRecruteur()"),
                  (2,3,"INSERT Recruteur"), (3,2,"id"), (2,1,"id"),
                  (1,2,"InsertContact()"), (2,3,"INSERT Recruteur_Contact"),
                  (3,2,"ok"), (2,1,"ok"), (1,0,"redirect to list")]),
        "cls_title": "Sprint 4 class diagram - Administration",
        "uc_title": "Sprint 4 use case diagram - Administration",
    },
    {
        "title": "Sprint 5: Applications & CV upload",
        "actors": [("Candidate", "Candidate"), ("Recruiter", "Recruiter")],
        "cases": [("uc_apply", "Apply to offer"), ("uc_upload", "Upload CV"),
                  ("uc_cover", "Add cover message"), ("uc_viewapps", "View candidatures"),
                  ("uc_download", "Download CV")],
        "links": [("Candidate", "uc_apply"), ("Candidate", "uc_upload"),
                  ("Candidate", "uc_cover"), ("Recruiter", "uc_viewapps"),
                  ("Recruiter", "uc_download")],
        "include": [("uc_apply", "uc_upload")], "extend": [("uc_cover", "uc_apply")],
        "classes": [
            {"name": "Offre_Postuler", "attrs": ["+ id", "+ _idOffre", "+ _idProfil",
              "+ nomCV", "+ message", "+ DatePostuler"], "methods": ["+ submit()", "+ list()"]},
            {"name": "Profil", "attrs": ["+ id", "+ nom", "+ prenom", "+ adresseMail"], "methods": []},
            {"name": "Offre", "attrs": ["+ id", "+ titreOffre"], "methods": []},
            {"name": "CvStorage", "attrs": ["+ path", "+ allowedExt"], "methods": ["+ save()", "+ read()"]},
        ],
        "rel": [("Offre_Postuler", "Profil", "0..*", "1", "solid", "none"),
                ("Offre_Postuler", "Offre", "0..*", "1", "solid", "none"),
                ("Offre_Postuler", "CvStorage", "1", "1", "dashed", "none")],
        "seq1": ("Submit an application with CV upload",
                 ["Candidate", "JobController", "CvStorage", "JF.DAL", "SQL Server"],
                 [(0,1,"Apply(form, cvFile)"), (1,2,"save(cvFile)"),
                  (2,1,"cvPath"), (1,3,"InsertPostuler()"),
                  (3,4,"INSERT Offre_Postuler"), (4,3,"id"), (3,1,"id"),
                  (1,0,"application confirmed")]),
        "seq2": ("Recruiter downloads a candidate CV",
                 ["Recruiter", "JobController", "JF.DAL", "SQL Server", "CvStorage"],
                 [(0,1,"ViewCV(applicationId)"), (1,2,"GetApplication(id)"),
                  (2,3,"SELECT Offre_Postuler"), (3,2,"app"),
                  (2,1,"app"), (1,4,"read(cvPath)"), (4,1,"fileBytes"),
                  (1,0,"return CV file")]),
        "cls_title": "Sprint 5 class diagram - Applications",
        "uc_title": "Sprint 5 use case diagram - Applications",
    },
    {
        "title": "Sprint 6: Advanced search & pagination",
        "actors": [("Visitor", "Visitor"), ("Candidate", "Candidate")],
        "cases": [("uc_search", "Search by title"), ("uc_filter", "Filter sector/city"),
                  ("uc_page", "Paginate results"), ("uc_ajax", "Live AJAX refresh")],
        "links": [("Visitor", "uc_search"), ("Visitor", "uc_filter"),
                  ("Visitor", "uc_page"), ("Candidate", "uc_ajax")],
        "include": [], "extend": [("uc_filter", "uc_search"), ("uc_ajax", "uc_page")],
        "classes": [
            {"name": "DefaultController", "attrs": ["+ pageSize"], "methods": ["+ Index(page,search,city,sector)"]},
            {"name": "PagedList", "attrs": ["+ PageNumber", "+ PageCount", "+ TotalItemCount"], "methods": ["+ GetPage()"]},
            {"name": "Offre", "attrs": ["+ id", "+ titreOffre", "+ _idVille", "+ _idSecteurActivite"], "methods": []},
            {"name": "index-pagination.js", "attrs": ["+ onSearch()", "+ onFilter()"], "methods": ["+ loadPage()"]},
        ],
        "rel": [("DefaultController", "PagedList", "1", "1", "dashed", "none"),
                ("PagedList", "Offre", "1", "0..*", "solid", "none"),
                ("index-pagination.js", "DefaultController", "1", "0..*", "dashed", "none")],
        "seq1": ("Server-side paginated search",
                 ["Visitor", "DefaultController", "JF.DAL", "SQL Server"],
                 [(0,1,"Index(page=2,search='dev')"), (1,2,"ListOffers(filters,page)"),
                  (2,3,"SELECT Offre OFFSET ... FETCH"), (3,2,"page+total"),
                  (2,1,"IPagedList<Offre>"), (1,0,"render _JobList")]),
        "seq2": ("AJAX live filter refresh",
                 ["Candidate", "index-pagination.js", "DefaultController", "JF.DAL"],
                 [(0,1,"onSearch('dev')"), (1,1,"buildQuery()", "self"),
                  (1,2,"Index?search=dev (AJAX)"), (2,3,"ListOffers()"),
                  (3,2,"html fragment"), (2,1,"_JobList partial"),
                  (1,0,"swap #job-list-container")]),
        "cls_title": "Sprint 6 class diagram - Search & pagination",
        "uc_title": "Sprint 6 use case diagram - Search & pagination",
    },
    {
        "title": "Sprint 7: Dashboards & statistics",
        "actors": [("Recruiter", "Recruiter"), ("Admin", "Administrator")],
        "cases": [("uc_dash", "View recruiter dashboard"), ("uc_admindash", "View admin dashboard"),
                  ("uc_stats", "View statistics"), ("uc_recent", "View recent activity")],
        "links": [("Recruiter", "uc_dash"), ("Recruiter", "uc_recent"),
                  ("Admin", "uc_admindash"), ("Admin", "uc_stats")],
        "include": [("uc_dash", "uc_stats")], "extend": [],
        "classes": [
            {"name": "DashboardViewModel", "attrs": ["+ TotalOffers", "+ PublishedOffers",
              "+ DraftOffers", "+ TotalApplications", "+ RecentOffers", "+ RecentApplications"], "methods": ["+ build()"]},
            {"name": "DefaultController", "attrs": [], "methods": ["+ TableauBord()", "+ Index()"]},
            {"name": "Offre", "attrs": ["+ id", "+ isPublie"], "methods": []},
            {"name": "Offre_Postuler", "attrs": ["+ id", "+ DatePostuler"], "methods": []},
        ],
        "rel": [("DashboardViewModel", "Offre", "1", "0..*", "dashed", "none"),
                ("DashboardViewModel", "Offre_Postuler", "1", "0..*", "dashed", "none"),
                ("DefaultController", "DashboardViewModel", "1", "1", "dashed", "none")],
        "seq1": ("Build the recruiter dashboard",
                 ["Recruiter", "DefaultController", "JF.DAL", "SQL Server"],
                 [(0,1,"TableauBord()"), (1,2,"GetDashboardData(recruiterId)"),
                  (2,3,"COUNT(Offre) GROUP BY isPublie"), (3,2,"counts"),
                  (2,3,"COUNT(Offre_Postuler)"), (3,2,"appCount"),
                  (2,3,"SELECT TOP 5 Offre / Applications"), (3,2,"recent"),
                  (2,1,"DashboardViewModel"), (1,0,"render dashboard")]),
        "seq2": ("Build the administration dashboard",
                 ["Admin", "DefaultController", "JF.DAL", "SQL Server"],
                 [(0,1,"Index()"), (1,2,"GetAdminStats()"),
                  (2,3,"COUNT(Offre) / COUNT(Recruteur)"), (3,2,"stats"),
                  (2,1,"ViewBag"), (1,0,"render admin dashboard")]),
        "cls_title": "Sprint 7 class diagram - Dashboards",
        "uc_title": "Sprint 7 use case diagram - Dashboards",
    },
]


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    generated = []

    # F1 - global use case (two clusters to stay landscape)
    cand_cases = [c for c in GLOBAL_CASES if c[0] in
                  ("uc_search", "uc_view", "uc_register", "uc_activate",
                   "uc_login", "uc_profile", "uc_cv", "uc_apply",
                   "uc_search_adv", "uc_reset")]
    rec_cases = [c for c in GLOBAL_CASES if c[0] in
                 ("uc_create_offre", "uc_publish", "uc_manage_apps",
                  "uc_dash", "uc_moder_offre", "uc_moder_rec",
                  "uc_manage_rec", "uc_stats", "uc_history", "uc_sectors")]
    cand_links = [l for l in GLOBAL_LINKS if l[1] in set(c[0] for c in cand_cases)]
    rec_links = [l for l in GLOBAL_LINKS if l[1] in set(c[0] for c in rec_cases)]
    usecase_diagram_two(
        "fig_01.png",
        "Global use case diagram of the JobzFactory platform",
        GLOBAL_ACTORS,
        [("Candidate space", cand_cases, cand_links),
         ("Recruiter / Admin space", rec_cases, rec_links)],
        "JobzFactory Platform",
        include=GLOBAL_INCLUDE, extend=GLOBAL_EXTEND)
    generated.append("fig_01.png")

    # F2 - overall architecture
    arch_diagram("fig_02.png", "Overall architecture of the JobzFactory solution",
                 [("Client", ["Browser (HTML5 / CSS3 / JS)"]),
                  ("Web server (IIS)", ["JobzFactory MVC", "Recruteur MVC",
                                        "Administration MVC", "Profil MVC"]),
                  ("Business / Data layer", ["Controllers / BLL", "JF.DAL (ADO.NET)"]),
                  ("Database server", ["SQL Server"])],
                 [("Browser (HTML5 / CSS3 / JS)", "JobzFactory MVC", "HTTP / HTTPS"),
                  ("Browser (HTML5 / CSS3 / JS)", "Recruteur MVC", "HTTP / HTTPS"),
                  ("Browser (HTML5 / CSS3 / JS)", "Administration MVC", "HTTP / HTTPS"),
                  ("Browser (HTML5 / CSS3 / JS)", "Profil MVC", "HTTP / HTTPS"),
                  ("JobzFactory MVC", "Controllers / BLL", "call"),
                  ("Recruteur MVC", "Controllers / BLL", "call"),
                  ("Administration MVC", "Controllers / BLL", "call"),
                  ("Profil MVC", "Controllers / BLL", "call"),
                  ("Controllers / BLL", "JF.DAL (ADO.NET)", "use"),
                  ("JF.DAL (ADO.NET)", "SQL Server", "T-SQL")])
    generated.append("fig_02.png")

    # F3 - deployment
    deployment_diagram("fig_03.png",
                       "Deployed deployment diagram of the JobzFactory platform",
                       [("client", "Client Browser", ["HTML5 / CSS3 / JS"]),
                        ("iis", "Windows Server - IIS",
                         ["JobzFactory site", "Recruteur site",
                          "Administration site", "Profil site",
                          "JF.DAL (ADO.NET)"]),
                        ("sql", "Database Server", ["SQL Server instance"]),
                        ("smtp", "Mail Server", ["SMTP relay"])],
                       [("client", "iis", "HTTP"),
                        ("iis", "sql", "TDS / T-SQL"),
                        ("iis", "smtp", "SMTP"),
                        ("smtp", "client", "activation email")])
    generated.append("fig_03.png")

    # F8 - global class diagram
    class_diagram("fig_08.png",
                  "Global class diagram of the JobzFactory platform",
                  global_class_classes(), GLOBAL_CLASS_REL)
    generated.append("fig_08.png")

    # F9 - global use case (design view) - two clusters, design framing
    usecase_diagram_two(
        "fig_09.png",
        "Global use case diagram (design view)",
        GLOBAL_ACTORS,
        [("Candidate space", cand_cases, cand_links),
         ("Recruiter / Admin space", rec_cases, rec_links)],
        "JobzFactory Platform (design)",
        include=GLOBAL_INCLUDE, extend=GLOBAL_EXTEND)
    generated.append("fig_09.png")

    # F10 - package diagram
    package_diagram("fig_10.png",
                    "Package / architecture diagram of the solution",
                    [("jf", "JobzFactory", "presentation"),
                     ("rec", "Recruteur", "presentation"),
                     ("adm", "Administration", "presentation"),
                     ("pro", "Profil", "presentation"),
                     ("dal", "JF.DAL", "data access"),
                     ("sql", "SQL Server", "database")],
                    [("jf", "dal"), ("rec", "dal"), ("adm", "dal"),
                     ("pro", "dal"), ("dal", "sql")])
    generated.append("fig_10.png")

    # Sprints: 4 diagrams each starting at figure 11
    base = 11
    for i, sp in enumerate(SPRINTS):
        n = base + i * 4
        usecase_diagram("fig_%02d.png" % n, sp["uc_title"], sp["actors"],
                        sp["cases"], sp["links"], sp["title"],
                        include=sp["include"], extend=sp["extend"])
        class_diagram("fig_%02d.png" % (n + 1), sp["cls_title"],
                      sp["classes"], sp["rel"])
        sequence_diagram("fig_%02d.png" % (n + 2), sp["seq1"][0],
                         sp["seq1"][1], sp["seq1"][2])
        sequence_diagram("fig_%02d.png" % (n + 3), sp["seq2"][0],
                         sp["seq2"][1], sp["seq2"][2])
        generated += ["fig_%02d.png" % n, "fig_%02d.png" % (n + 1),
                      "fig_%02d.png" % (n + 2), "fig_%02d.png" % (n + 3)]

    print("Generated %d diagrams:" % len(generated))
    for f in generated:
        p = os.path.join(ASSETS, f)
        print("  %s  %s" % (f, "OK" if os.path.exists(p) else "MISSING"))


if __name__ == "__main__":
    main()
