document.addEventListener('DOMContentLoaded', function() {
    console.log('Packages.js loaded');
    initializePackageItemsManagement();
    initializeFormHandlers();
});

function initializePackageItemsManagement() {
    // Add Package Item functionality
    document.addEventListener('click', function(e) {
        var addButton = e.target.closest('.add-package-item');
        if (addButton) {
            e.preventDefault();
            console.log('Add item button clicked');
            var container = addButton.closest('.card-body').querySelector('[id^="packageItems"], [id^="editPackageItems-"]');
            if (container) {
                addPackageItemRow(container);
            }
        }
    });

    // Remove Package Item functionality
    document.addEventListener('click', function(e) {
        var removeButton = e.target.closest('.remove-item');
        if (removeButton) {
            e.preventDefault();
            console.log('Remove item button clicked');
            var row = removeButton.closest('.package-item-row');
            var container = row.parentElement;
            
            if (container.querySelectorAll('.package-item-row').length > 1) {
                row.remove();
                reindexPackageItems(container.closest('form'));
            } else {
                alert('At least one item is required for the package.');
            }
        }
    });

    // Item selection change handler
    document.addEventListener('change', function(e) {
        if (e.target.matches('select[name*="ItemId"], .item-select')) {
            console.log('Item select changed:', e.target.value);
            updateItemStock(e.target);
        }
    });

    // Quantity input change handler
    document.addEventListener('input', function(e) {
        if (e.target.matches('input[name*="ItemQuantity"], .item-quantity')) {
            validateQuantityInput(e.target);
        }
    });

    // Initialize existing selects after a short delay to ensure DOM is ready
    setTimeout(function() {
        console.log('Initializing existing item selects...');
        document.querySelectorAll('select[name*="ItemId"], .item-select').forEach(function(select) {
            if (select.value && select.selectedIndex > 0) {
                console.log('Initializing select with value:', select.value);
                updateItemStock(select);
            }
        });
    }, 200);
}

function initializeFormHandlers() {
    // Form submission handler for reindexing PackageItems
    document.addEventListener('submit', function(e) {
        var form = e.target;
        if (form.id && (form.id.startsWith('editPackageForm-') || form.id === 'addPackageForm')) {
            console.log('Form submitting, reindexing items...');
            reindexPackageItems(form);
        }
    });
}

function addPackageItemRow(container) {
    console.log('Adding package item row');
    var existingRows = container.querySelectorAll('.package-item-row');
    if (existingRows.length === 0) {
        console.error('No existing rows found to clone');
        return;
    }
    
    var newIndex = existingRows.length;
    var templateRow = existingRows[0].cloneNode(true);
    
    // Clear values and update names
    templateRow.querySelectorAll('select, input').forEach(function(element) {
        if (element.name.includes('ItemId')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '';
            element.selectedIndex = 0;
        } else if (element.name.includes('ItemQuantity')) {
            element.name = element.name.replace(/\[\d+\]/, '[' + newIndex + ']');
            element.value = '1';
        }
    });

    // Clear stock info
    var stockInfo = templateRow.querySelector('.available-stock');
    if (stockInfo) {
        stockInfo.textContent = '';
        stockInfo.className = 'available-stock form-text text-muted';
    }

    container.appendChild(templateRow);
    console.log('New row added with index:', newIndex);
}

function updateItemStock(selectElement) {
    var selectedOption = selectElement.options[selectElement.selectedIndex];
    var row = selectElement.closest('.package-item-row');
    var stockInfo = row.querySelector('.available-stock');
    var quantityInput = row.querySelector('input[name*="ItemQuantity"], .item-quantity');
    
    console.log('Updating stock for select:', selectElement.name);
    console.log('Selected option:', selectedOption ? selectedOption.textContent : 'none');
    
    if (!selectedOption || selectedOption.value === '') {
        // Clear stock info if no item selected
        if (stockInfo) {
            stockInfo.textContent = '';
            stockInfo.className = 'available-stock form-text text-muted';
        }
        if (quantityInput) {
            quantityInput.disabled = false;
            quantityInput.max = '';
            quantityInput.value = '1';
        }
        return;
    }
    
    var stockAttr = selectedOption.getAttribute('data-stock');
    var stock = parseInt(stockAttr) || 0;
    
    console.log('Stock attribute:', stockAttr, 'Parsed stock:', stock);
    
    if (stockInfo) {
        if (stock > 0) {
            stockInfo.textContent = 'Available: ' + stock;
            stockInfo.className = 'available-stock form-text text-success';
        } else {
            stockInfo.textContent = 'Out of stock';
            stockInfo.className = 'available-stock form-text text-danger';
        }
    }
    
    if (quantityInput) {
        if (stock > 0) {
            quantityInput.disabled = false;
            quantityInput.max = stock;
            
            // Set a reasonable default if current value is invalid
            var currentValue = parseInt(quantityInput.value) || 0;
            if (currentValue <= 0) {
                quantityInput.value = '1';
            } else if (currentValue > stock) {
                quantityInput.value = stock.toString();
            }
        } else {
            quantityInput.disabled = true;
            quantityInput.value = '0';
            quantityInput.max = '0';
        }
    }
}

function validateQuantityInput(quantityInput) {
    var row = quantityInput.closest('.package-item-row');
    var select = row.querySelector('select[name*="ItemId"], .item-select');
    
    if (!select || !select.value) return;
    
    var selectedOption = select.options[select.selectedIndex];
    var stock = parseInt(selectedOption.getAttribute('data-stock')) || 0;
    var quantity = parseInt(quantityInput.value) || 0;
    var stockInfo = row.querySelector('.available-stock');
    
    if (quantity > stock) {
        quantityInput.value = stock.toString();
        if (stockInfo) {
            stockInfo.textContent = 'Quantity reduced to available stock: ' + stock;
            stockInfo.className = 'available-stock form-text text-warning';
        }
        
        setTimeout(function() {
            if (stockInfo) {
                stockInfo.textContent = 'Available: ' + stock;
                stockInfo.className = 'available-stock form-text text-success';
            }
        }, 2000);
    } else if (quantity <= 0) {
        quantityInput.value = '1';
    }
}

function reindexPackageItems(form) {
    if (!form) {
        console.error('No form provided for reindexing');
        return;
    }
    
    console.log('Reindexing package items in form:', form.id);
    var rows = form.querySelectorAll('.package-item-row');
    rows.forEach(function(row, index) {
        row.querySelectorAll('select, input').forEach(function(element) {
            if (element.name && element.name.includes('PackageItems')) {
                var oldName = element.name;
                element.name = element.name.replace(/\[\d+\]/, '[' + index + ']');
                console.log('Renamed:', oldName, '->', element.name);
            }
        });
    });
}

// Utility function to debug selects
function debugSelects() {
    console.log('=== DEBUG: All Item Selects ===');
    document.querySelectorAll('select[name*="ItemId"], .item-select').forEach(function(select, index) {
        console.log('Select ' + index + ':', {
            name: select.name,
            value: select.value,
            options: Array.from(select.options).map(opt => ({
                value: opt.value,
                text: opt.textContent,
                stock: opt.getAttribute('data-stock')
            }))
        });
    });
}

// Call debug function for troubleshooting (remove in production)
setTimeout(debugSelects, 1000);
