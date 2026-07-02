#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Read a .docx file: print all paragraphs with their style names, count
images/tables, and extract embedded images to an output folder.

Usage:
    python read_docx.py <docx_path> <output_folder>
"""
import sys
import os
import argparse

try:
    from docx import Document
    from docx.oxml.ns import qn
except ImportError:
    sys.exit("python-docx is not installed. Run: python -m pip install python-docx")


def iter_block_items(parent):
    """Yield paragraphs and tables in document order."""
    from docx.document import Document as _Doc
    from docx.table import Table
    from docx.text.paragraph import Paragraph
    if isinstance(parent, _Doc):
        parent_elm = parent.element.body
    else:
        parent_elm = parent._element
    for child in parent_elm.iterchildren():
        if child.tag == qn("w:p"):
            yield Paragraph(child, parent)
        elif child.tag == qn("w:tbl"):
            yield Table(child, parent)


def read_docx(docx_path: str, output_folder: str) -> None:
    if not os.path.isfile(docx_path):
        raise FileNotFoundError(f"DOCX not found: {docx_path}")
    os.makedirs(output_folder, exist_ok=True)

    doc = Document(docx_path)

    print("=" * 70)
    print(f"DOCX: {docx_path}")
    print("=" * 70)

    # Paragraphs (with style names) in document order, plus tables interleaved
    para_count = 0
    table_count = 0
    print("\n--- PARAGRAPHS / BLOCKS (in order) ---")
    for block in iter_block_items(doc):
        from docx.table import Table
        from docx.text.paragraph import Paragraph
        if isinstance(block, Paragraph):
            para_count += 1
            style = block.style.name if block.style else "(none)"
            text = block.text
            marker = ""
            if text.strip() == "":
                marker = " [empty]"
            print(f"[P{para_count:03d}] (style={style}){marker}: {text}")
        elif isinstance(block, Table):
            table_count += 1
            print(f"\n[TABLE #{table_count}] rows={len(block.rows)} cols={len(block.columns)}")
            for r_i, row in enumerate(block.rows):
                cells = [c.text.replace("\n", " | ") for c in row.cells]
                print(f"   row{r_i}: {cells}")
            print()

    print("\n--- SUMMARY ---")
    print(f"Paragraphs: {para_count}")
    print(f"Tables    : {table_count}")

    # Count & extract embedded images
    img_parts = doc.part.related_parts
    image_count = 0
    extracted = []
    for rel_id, part in img_parts.items():
        if "image" in part.content_type:
            image_count += 1
            ext = os.path.splitext(part.partname)[1] or ".bin"
            fname = f"image_{image_count:03d}{ext}"
            fpath = os.path.join(output_folder, fname)
            with open(fpath, "wb") as f:
                f.write(part.blob)
            extracted.append(fpath.replace("\\", "/"))

    print(f"Images    : {image_count}")
    if extracted:
        print("Extracted images:")
        for p in extracted:
            print(f"   {p}")

    # Sections / page setup info (margins, page size)
    print("\n--- SECTIONS / PAGE SETUP ---")
    for i, section in enumerate(doc.sections):
        print(f"Section {i+1}:")
        print(f"  page_width  = {section.page_width}  page_height = {section.page_height}")
        print(f"  margins L/R/T/B = {section.left_margin}/{section.right_margin}/{section.top_margin}/{section.bottom_margin}")
        print(f"  header_distance = {section.header_distance}  footer_distance = {section.footer_distance}")

    # Styles used
    print("\n--- STYLES DEFINED ---")
    for s in doc.styles:
        try:
            print(f"  {s.type}: {s.name}")
        except Exception:
            pass


def main():
    parser = argparse.ArgumentParser(description="Read & dump a .docx file")
    parser.add_argument("docx_path", help="Path to the .docx file")
    parser.add_argument("output_folder", help="Folder for extracted images")
    args = parser.parse_args()
    read_docx(args.docx_path, args.output_folder)


if __name__ == "__main__":
    main()
