# -*- coding: utf-8 -*-
"""Open the generated DOCX in Word, update fields + TOC, report page count."""
import os
import win32com.client as win32

DOCX = os.path.abspath(os.path.join(os.path.dirname(__file__), "..",
                                    "Report", "JobzFactory_Rapport.docx"))

word = win32.Dispatch("Word.Application")
word.Visible = False
try:
    doc = word.Documents.Open(DOCX, ReadOnly=False)
    # update all fields (figure refs, etc.)
    try:
        doc.Fields.Update()
    except Exception as e:
        print("Fields.Update warn:", e)
    # update tables of contents
    for toc in doc.TablesOfContents:
        toc.Update()
    # update again so TOC page numbers settle
    try:
        doc.Fields.Update()
    except Exception:
        pass
    pages = doc.ComputeStatistics(2)  # wdStatisticPages = 2
    words = doc.ComputeStatistics(0)  # wdStatisticWords
    print("PAGES:", pages)
    print("WORDS:", words)
    doc.Save()
    doc.Close(False)
finally:
    word.Quit()
print("DONE")
