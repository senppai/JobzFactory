# -*- coding: utf-8 -*-
import os, sys, time
import win32com.client as win32

path = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "Report", "JobzFactory_Rapport.docx"))
print("DOCX:", path)
print("Size (bytes):", os.path.getsize(path))

word = win32.Dispatch("Word.Application")
word.Visible = False
try:
    doc = word.Documents.Open(path, ReadOnly=False)
    # Update all fields (TOC, PAGE, etc.) so page count is accurate
    try:
        doc.Fields.Update()
    except Exception as e:
        print("Fields.Update warning:", e)
    try:
        for t in doc.TablesOfContents:
            t.Update()
    except Exception as e:
        print("TOC update warning:", e)
    # Word enumeration for wdStatisticPages = 2
    pages = doc.ComputeStatistics(2)
    words = doc.ComputeStatistics(0)
    print("Pages:", pages)
    print("Words:", words)
    doc.Close(False)
finally:
    word.Quit()
