#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Render every page of a PDF to a PNG image using PyMuPDF (fitz).

Usage:
    python pdf_to_images.py <pdf_path> <output_folder> [--dpi 200]
"""
import sys
import os
import json
import argparse

try:
    import fitz  # PyMuPDF
except ImportError:
    sys.exit("PyMuPDF (fitz) is not installed. Run: python -m pip install PyMuPDF")


def render_pdf(pdf_path: str, output_folder: str, dpi: int = 200) -> dict:
    if not os.path.isfile(pdf_path):
        raise FileNotFoundError(f"PDF not found: {pdf_path}")

    os.makedirs(output_folder, exist_ok=True)

    # 72 DPI is the PDF base; zoom = dpi / 72
    zoom = dpi / 72.0
    matrix = fitz.Matrix(zoom, zoom)

    doc = fitz.open(pdf_path)
    total = doc.page_count
    width = len(str(total))

    files = []
    for i, page in enumerate(doc, start=1):
        pixmap = page.get_pixmap(matrix=matrix)
        fname = f"page_{str(i).zfill(max(width, 3))}.png"
        fpath = os.path.join(output_folder, fname)
        pixmap.save(fpath)
        files.append(fpath.replace("\\", "/"))
        print(f"[{i}/{total}] {fname}  ({pixmap.width}x{pixmap.height})")

    manifest = {
        "pdf": os.path.abspath(pdf_path).replace("\\", "/"),
        "output_folder": os.path.abspath(output_folder).replace("\\", "/"),
        "dpi": dpi,
        "total_pages": total,
        "files": files,
    }
    manifest_path = os.path.join(output_folder, "manifest.json")
    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    print(f"\nDone. {total} pages rendered to {output_folder}")
    print(f"Manifest: {manifest_path}")
    return manifest


def main():
    parser = argparse.ArgumentParser(description="PDF -> PNG images via PyMuPDF")
    parser.add_argument("pdf_path", help="Path to the PDF file")
    parser.add_argument("output_folder", help="Folder where PNGs will be written")
    parser.add_argument("--dpi", type=int, default=200, help="Render DPI (default 200)")
    args = parser.parse_args()
    render_pdf(args.pdf_path, args.output_folder, args.dpi)


if __name__ == "__main__":
    main()
