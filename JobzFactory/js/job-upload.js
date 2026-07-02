(function () {
    'use strict';

    var MAX_SIZE = 10 * 1024 * 1024; // 10 MB
    var ALLOWED = ['.pdf', '.doc', '.docx'];

    function formatSize(bytes) {
        if (bytes < 1024) return bytes + ' B';
        if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
        return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    }

    function isValidFile(file) {
        var name = file.name.toLowerCase();
        var ext = name.substring(name.lastIndexOf('.'));
        if (ALLOWED.indexOf(ext) === -1) {
            alert('Please upload a PDF, DOC, or DOCX file.');
            return false;
        }
        if (file.size > MAX_SIZE) {
            alert('File is too large. Maximum size is 10 MB.');
            return false;
        }
        return true;
    }

    function init() {
        var container = document.getElementById('cv-upload');
        if (!container) return;

        var input = document.getElementById('fichieCV');
        var zone = document.getElementById('cv-upload-zone');
        var fileBar = document.getElementById('cv-upload-file');
        var fileName = document.getElementById('cv-file-name');
        var fileSize = document.getElementById('cv-file-size');
        var removeBtn = document.getElementById('cv-upload-remove');

        function showFile(file) {
            fileName.textContent = file.name;
            fileSize.textContent = formatSize(file.size);
            fileBar.hidden = false;
            container.classList.add('has-file');
        }

        function clearFile() {
            input.value = '';
            fileBar.hidden = true;
            container.classList.remove('has-file');
        }

        function handleFile(file) {
            if (!file || !isValidFile(file)) {
                clearFile();
                return;
            }
            showFile(file);
        }

        zone.addEventListener('click', function () {
            input.click();
        });

        zone.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                input.click();
            }
        });

        input.addEventListener('change', function () {
            if (input.files && input.files[0]) {
                handleFile(input.files[0]);
            }
        });

        removeBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            clearFile();
        });

        ['dragenter', 'dragover'].forEach(function (eventName) {
            zone.addEventListener(eventName, function (e) {
                e.preventDefault();
                e.stopPropagation();
                zone.classList.add('is-dragover');
            });
        });

        ['dragleave', 'drop'].forEach(function (eventName) {
            zone.addEventListener(eventName, function (e) {
                e.preventDefault();
                e.stopPropagation();
                zone.classList.remove('is-dragover');
            });
        });

        zone.addEventListener('drop', function (e) {
            var files = e.dataTransfer && e.dataTransfer.files;
            if (files && files[0]) {
                if (isValidFile(files[0])) {
                    var dt = new DataTransfer();
                    dt.items.add(files[0]);
                    input.files = dt.files;
                    showFile(files[0]);
                }
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
