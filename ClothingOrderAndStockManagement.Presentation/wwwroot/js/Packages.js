document.addEventListener('DOMContentLoaded', function () {
    // Initialize modal handling
    initializeModalHandling();

    // Initialize package items management
    initializePackageItemsManagement();

    // Initialize form handlers
    initializeFormHandlers();
});

function initializeModalHandling() {
    // Auto-open Add Package Modal on validation errors
    var showAddModal = document.getElementById('showAddModal');
    if (showAddModal && showAddModal.value === 'true') {
        var addPackageModal = new bootstrap.Modal(document.getElementById('addPackageModal'));
        addPackageModal.show();
    }

    // Auto-open Edit Package Modal on validation errors
    var showEditModalId = document.getElementById('showEditModalId');
    if (showEditModalId && showEditModalId.value) {
        var editModalId = 'editPackageModal-' + showEditModalId.value;
        var editModal = document.getElementById(editModalId);
        if (editModal) {
            var editPackageModal = new bootstrap.Modal(editModal);
            editPackageModal.show();
        }
    }
}

function initializePackageItemsManagement() {
    // Add Package Item functionality
    document.addEventListener('click', function (e) {
        var addButton = e.target.closest('.add-package-item');
        if (addButton) {
            e.preventDefault();
            var container = addButton.closest('.card-body').querySelector('[id^="packageItems"], [id^="editPackageItems-"]');
            addPackageItemRow(container);
        }
    });

    // Remove Package Item functionality
    document.addEventListener('click', function (e) {
        var removeButton = e.target.closest('.remove-item');
        if (removeButton) {
            e.preventDefault();
            var row = removeButton.closest('.package-item-row');
            var container = row.parentElement;

            if (container.querySelectorAll('.package-item-row').length > 1) {
                row.remove();
            } else {
                alert('At least one item is required for the package.');
            }
        }
    });

    // Item selection change handler
    document.addEventListener('change', function (e) {
        if (e.target.matches('select[name*="ItemId"]')) {
            updateItemStock(e.target);
        }
    });
}

function initializeFormHandlers() {
    // Form submission handler for reindexing PackageItems
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form.id && (form.id.startsWith('editPackageForm-') || form.id === 'addPackageForm')) {
            reindexPackageItems(form);
        }
    });
}

function addPackageItemRow(container) {
    var existingRows = container.querySelectorAll('.package-item-row');
    var newIndex = existingRows.length;
    var templateRow = existingRows[0].cloneNode(true);

    // Clear values and update names
    templateRow.querySelectorAll('select, input').forEach(function (element) {
        if (element.name.includes('ItemId')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '';
        } else if (element.name.includes('ItemQuantity')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '';
        }
    });

    // Clear stock info
    var stockInfo = templateRow.querySelector('.available-stock');
    if (stockInfo) {
        stockInfo.textContent = '';
    }

    container.appendChild(templateRow);
}

function updateItemStock(selectElement) {
    var selectedOption = selectElement.options[selectElement.selectedIndex];
    var stock = selectedOption.getAttribute('data-stock') || '0';
    var row = selectElement.closest('.package-item-row');
    var stockInfo = row.querySelector('.available-stock');
    var quantityInput = row.querySelector('.item-quantity');

    if (stockInfo) {
        stockInfo.textContent = 'Available: ' + stock;
    }

    if (quantityInput) {
        quantityInput.max = stock;
        if (parseInt(quantityInput.value) > parseInt(stock)) {
            quantityInput.value = stock;
        }
    }
}

function reindexPackageItems(form) {
    var rows = form.querySelectorAll('.package-item-row');
    rows.forEach(function (row, index) {
        row.querySelectorAll('select, input').forEach(function (element) {
            if (element.name.includes('PackageItems')) {
                element.name = element.name.replace(/\[\d+\]/, '[' + index + ']');
            }
        });
    });
}
