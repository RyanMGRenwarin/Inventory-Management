/**
 * SweetAlert2 Delete Confirmation
 * Used for all delete actions in the app
 */

function confirmDelete(event, options) {
    event.preventDefault();

    var defaultOptions = {
        title: 'Are you sure?',
        text: "This action cannot be undone!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel'
    };

    // Merge with custom options
    var settings = Object.assign({}, defaultOptions, options);

    Swal.fire(settings).then((result) => {
        if (result.isConfirmed) {
            // Submit form
            var form = event.target.closest('form');
            if (form) {
                form.submit();
            } else {
                // if it is not a form (link), redirect
                var url = event.target.getAttribute('href');
                if (url) {
                    window.location.href = url;
                }
            }
        }
    });
}

// Attach to all delete buttons with class 'delete-btn'
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.delete-btn').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            var title = this.dataset.title || 'Are you sure?';
            var text = this.dataset.text || "This action cannot be undone!";
            var confirmText = this.dataset.confirmText || 'Yes, delete it!';
            var cancelText = this.dataset.cancelText || 'Cancel';

            confirmDelete(e, {
                title: title,
                text: text,
                confirmButtonText: confirmText,
                cancelButtonText: cancelText
            });
        });
    });
});
