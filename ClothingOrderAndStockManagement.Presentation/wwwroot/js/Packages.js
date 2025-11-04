document.addEventListener('DOMContentLoaded', function () {
    initializePackageItemsManagement();
    initializeFormHandlers();
});

function initializePackageItemsManagement() {
    // Add Package Item functionality
    document.addEventListener('click', function (e) {
        var addButton = e.target.closest('.add-package-item');
        if (addButton) {
            e.preventDefault();
            var container = addButton.closest('.card-body').querySelector('[id^="packageItems"], [id^="editPackageItems-"]');
            if (container) {
                addPackageItemRow(container);
            }
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
                // Reindex after removal
                reindexPackageItems(container.closest('form'));
            } else {
                alert('At least one item is required for the package.');
            }
        }
    });

    // Item selection change handler for stock info
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
    if (existingRows.length === 0) return;

    var newIndex = existingRows.length;
    var templateRow = existingRows[0].cloneNode(true);

    // Clear values and update names
    templateRow.querySelectorAll('select, input').forEach(function (element) {
        if (element.name.includes('ItemId')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '';
            element.selectedIndex = 0;
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
    var quantityInput = row.querySelector('.item-quantity, input[name*="ItemQuantity"]');

    if (stockInfo) {
        stockInfo.textContent = stock > 0 ? 'Available: ' + stock : 'Out of stock';
        stockInfo.className = stock > 0 ? 'form-text text-muted' : 'form-text text-danger';
    }

    if (quantityInput) {
        quantityInput.max = stock;
        if (parseInt(quantityInput.value) > parseInt(stock)) {
            quantityInput.value = Math.max(1, parseInt(stock));
        }
    }
}

function reindexPackageItems(form) {
    if (!form) return;

    var rows = form.querySelectorAll('.package-item-row');
    rows.forEach(function (row, index) {
        row.querySelectorAll('select, input').forEach(function (element) {
            if (element.name.includes('PackageItems')) {
                element.name = element.name.replace(/\[\d+\]/, '[' + index + ']');
            }
        });
    });
}

// Initialize any existing package item selects on page load
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('select[name*="ItemId"]').forEach(function (select) {
        if (select.value) {
            updateItemStock(select);
        }
    });
});
