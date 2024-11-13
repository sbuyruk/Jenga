document.querySelectorAll('.open-confirm-modal').forEach(button => {
    button.addEventListener('click', function () {
        const itemId = this.getAttribute('data-id'); // Item ID for deletion
        const deleteUrl = this.getAttribute('data-url'); // URL to delete the item
        const modalTitle = this.getAttribute('data-title') || 'Delete Confirmation'; // Custom title or default
        const modalMessage = this.getAttribute('data-message') || 'Are you sure you want to delete this item?'; // Custom message or default

        // Set form action, item ID, modal title, and modal message
        document.getElementById('deleteForm').setAttribute('action', deleteUrl);
        document.getElementById('deleteItemId').value = itemId;
        document.getElementById('confirmationModalTitle').innerHTML = modalTitle;
        document.getElementById('confirmationModalMessage').innerHTML = modalMessage;

        // Show the modal
        new bootstrap.Modal(document.getElementById('confirmationModal')).show();
    });
});
