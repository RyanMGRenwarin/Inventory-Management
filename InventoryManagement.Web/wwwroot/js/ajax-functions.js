/**
 * AJAX Functions for Inventory Management
 */

$(document).ready(function () {
    // ============================================================
    // 1. LOAD PARTIAL VIEW VIA AJAX
    // ============================================================
    $('.load-partial').on('click', function (e) {
        e.preventDefault();
        var url = $(this).data('url');
        var target = $(this).data('target');

        $.ajax({
            url: url,
            type: 'GET',
            beforeSend: function () {
                $(target).html('<div class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');
            },
            success: function (data) {
                $(target).html(data);
            },
            error: function (xhr, status, error) {
                $(target).html('<div class="alert alert-danger">Error loading content: ' + error + '</div>');
            }
        });
    });

    // ============================================================
    // 2. AJAX FORM SUBMIT (Create/Edit)
    // ============================================================
    $('.ajax-form').on('submit', function (e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var method = form.attr('method') || 'POST';
        var target = form.data('target');
        var formData = new FormData(this);

        // Disable submit button
        var submitBtn = form.find('button[type="submit"]');
        submitBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');

        $.ajax({
            url: url,
            type: method,
            data: formData,
            processData: false,
            contentType: false,
            success: function (data) {
                if (target) {
                    $(target).html(data);
                } else {
                    // Reload page or redirect
                    window.location.href = data.redirectUrl || window.location.href;
                }
                showToast('success', 'Operation completed successfully!');
            },
            error: function (xhr, status, error) {
                if (xhr.status === 400) {
                    // Validation errors - update form with errors
                    if (target) {
                        $(target).html(xhr.responseText);
                    }
                } else {
                    showToast('error', 'An error occurred: ' + (xhr.responseJSON?.message || error));
                }
            },
            complete: function () {
                submitBtn.prop('disabled', false).html('Save');
            }
        });
    });

    // ============================================================
    // 3. AJAX DELETE WITH CONFIRMATION
    // ============================================================
    $('.ajax-delete').on('click', function (e) {
        e.preventDefault();
        var url = $(this).data('url');
        var itemName = $(this).data('name') || 'item';
        var target = $(this).data('target');

        if (!confirm('Are you sure you want to delete this ' + itemName + '? This action cannot be undone.')) {
            return;
        }

        $.ajax({
            url: url,
            type: 'POST',
            beforeSend: function () {
                // Show loading
                $(this).prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
            }.bind(this),
            success: function (data) {
                if (target) {
                    // Reload the list
                    $.ajax({
                        url: target,
                        type: 'GET',
                        success: function (html) {
                            $(target).html(html);
                        }
                    });
                } else {
                    window.location.reload();
                }
                showToast('success', itemName + ' deleted successfully!');
            },
            error: function (xhr, status, error) {
                showToast('error', 'Failed to delete ' + itemName + ': ' + (xhr.responseJSON?.message || error));
            },
            complete: function () {
                $(this).prop('disabled', false).html('Delete');
            }.bind(this)
        });
    });

    // ============================================================
    // 4. AJAX SEARCH / FILTER
    // ============================================================
    $('.ajax-search').on('input', function () {
        var searchTerm = $(this).val();
        var url = $(this).data('url');
        var target = $(this).data('target');
        var delay = $(this).data('delay') || 300;

        clearTimeout($(this).data('timer'));
        $(this).data('timer', setTimeout(function () {
            $.ajax({
                url: url,
                type: 'GET',
                data: { searchTerm: searchTerm },
                beforeSend: function () {
                    $(target).html('<div class="text-center"><div class="spinner-border spinner-border-sm" role="status"><span class="visually-hidden">Loading...</span></div></div>');
                },
                success: function (data) {
                    $(target).html(data);
                },
                error: function (xhr, status, error) {
                    $(target).html('<div class="alert alert-danger">Search failed: ' + error + '</div>');
                }
            });
        }, delay));
    });

    // ============================================================
    // 5. AJAX LOAD DROPDOWN (Chained Dropdowns)
    // ============================================================
    $('.chained-dropdown').on('change', function () {
        var selectedValue = $(this).val();
        var targetDropdown = $($(this).data('target'));
        var url = $(this).data('url');

        if (!selectedValue) {
            targetDropdown.html('<option value="">Select...</option>');
            return;
        }

        $.ajax({
            url: url,
            type: 'GET',
            data: { id: selectedValue },
            beforeSend: function () {
                targetDropdown.html('<option value="">Loading...</option>');
            },
            success: function (data) {
                targetDropdown.html(data);
            },
            error: function (xhr, status, error) {
                targetDropdown.html('<option value="">Error loading data</option>');
                showToast('error', 'Failed to load data: ' + error);
            }
        });
    });

    // ============================================================
    // 6. TOAST NOTIFICATION SYSTEM
    // ============================================================
    function showToast(type, message) {
        var toastHtml = `
            <div class="toast-container position-fixed bottom-0 end-0 p-3">
                <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="d-flex">
                        <div class="toast-body">
                            ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            </div>
        `;

        // Remove existing toasts
        $('.toast-container').remove();

        // Add new toast
        $('body').append(toastHtml);

        // Auto-hide after 5 seconds
        setTimeout(function () {
            $('.toast-container').fadeOut(500, function () {
                $(this).remove();
            });
        }, 5000);
    }

    // ============================================================
    // 7. AUTO-REFRESH PARTIAL (Real-time updates)
    // ============================================================
    $('.auto-refresh').each(function () {
        var interval = $(this).data('interval') || 30000; // Default 30 seconds
        var url = $(this).data('url');
        var target = $(this);

        setInterval(function () {
            $.ajax({
                url: url,
                type: 'GET',
                success: function (data) {
                    target.html(data);
                },
                error: function (xhr, status, error) {
                    // Silent fail for auto-refresh
                    console.log('Auto-refresh failed:', error);
                }
            });
        }, interval);
    });

    // ============================================================
    // 8. AJAX FILE UPLOAD (with progress)
    // ============================================================
    $('.ajax-upload').on('change', function () {
        var fileInput = this;
        var url = $(this).data('url');
        var target = $(this).data('target');

        if (fileInput.files.length === 0) {
            return;
        }

        var formData = new FormData();
        formData.append('file', fileInput.files[0]);

        // Show progress bar
        var progressHtml = `
            <div class="progress">
                <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">0%</div>
            </div>
        `;
        $(target).html(progressHtml);

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            xhr: function () {
                var xhr = new XMLHttpRequest();
                xhr.upload.addEventListener('progress', function (e) {
                    if (e.lengthComputable) {
                        var percent = Math.round((e.loaded / e.total) * 100);
                        $('.progress-bar').css('width', percent + '%').attr('aria-valuenow', percent).text(percent + '%');
                    }
                });
                return xhr;
            },
            success: function (data) {
                $(target).html('<div class="alert alert-success">File uploaded successfully!</div>');
                showToast('success', 'File uploaded successfully!');
            },
            error: function (xhr, status, error) {
                $(target).html('<div class="alert alert-danger">Upload failed: ' + (xhr.responseJSON?.message || error) + '</div>');
                showToast('error', 'Upload failed!');
            }
        });
    });
});
