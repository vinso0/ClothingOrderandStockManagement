let packageCount = 1;

function addPackageRow() {
    const container = document.getElementById('packageContainer');
    const newRow = document.createElement('div');
    newRow.className = 'package-item';
    newRow.innerHTML = `
            <div class="row g-3">
                <div class="col-md-5">
                    <label class="form-label">Package</label>
                    <select class="form-select package-select" name="OrderPackages[${packageCount}].PackagesId" required onchange="updatePrice(this, ${packageCount})">
                        <option value="">Select Package</option>
                        @foreach (var package in packages)
                        {
                                        <option value="@package.PackagesId" data-price="@package.Price">
                                            @package.PackageName - ₱@package.Price.ToString("N2")
                                        </option>
                        }
                    </select>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Quantity</label>
                    <input type="number" class="form-control quantity-input"
                           name="OrderPackages[${packageCount}].Quantity"
                           min="1" value="1" required
                           onchange="updatePrice(this, ${packageCount})" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Subtotal</label>
                    <input type="text" class="form-control subtotal-display"
                           id="subtotal-${packageCount}" readonly value="₱0.00" />
                    <input type="hidden" name="OrderPackages[${packageCount}].PriceAtPurchase"
                           id="price-${packageCount}" value="0" />
                </div>
                <div class="col-md-1 d-flex align-items-end">
                    <button type="button" class="btn btn-remove" onclick="removePackageRow(this)" title="Remove package">
                        ×
                    </button>
                </div>
            </div>
        `;
    container.appendChild(newRow);
    packageCount++;
}

function removePackageRow(button) {
    button.closest('.package-item').remove();
    calculateTotal();
}

function updatePrice(element, index) {
    const row = element.closest('.package-item');
    const select = row.querySelector('.package-select');
    const quantityInput = row.querySelector('.quantity-input');
    const selectedOption = select.options[select.selectedIndex];
    const price = parseFloat(selectedOption.getAttribute('data-price') || 0);
    const quantity = parseInt(quantityInput.value) || 1;
    const subtotal = price * quantity;

    document.getElementById(`subtotal-${index}`).value = '₱' + subtotal.toFixed(2);
    document.getElementById(`price-${index}`).value = price;

    calculateTotal();
}

function calculateTotal() {
    let total = 0;
    document.querySelectorAll('.subtotal-display').forEach(input => {
        const value = parseFloat(input.value.replace('₱', '').replace(',', '')) || 0;
        total += value;
    });
    document.getElementById('totalAmount').textContent = '₱' + total.toFixed(2);
    calculateRemainingBalance();
}

function calculateRemainingBalance() {
    const totalText = document.getElementById('totalAmount').textContent;
    const total = parseFloat(totalText.replace('₱', '').replace(',', '')) || 0;
    const paid = parseFloat(document.getElementById('paymentAmount').value) || 0;
    const remaining = total - paid;
    document.getElementById('remainingBalance').value = '₱' + remaining.toFixed(2);
}

// SINGLE, CORRECTED displayFileName function
function displayFileName(input, number) {
    console.log('displayFileName called for input', number);
    const fileNameElement = document.getElementById('fileName' + number);

    if (!fileNameElement) {
        console.error('File name element not found:', 'fileName' + number);
        return;
    }

    if (input.files && input.files[0]) {
        const file = input.files[0];
        console.log('File selected:', file.name, 'Size:', file.size, 'Type:', file.type);

        // Validate file size (10MB)
        if (file.size > 10 * 1024 * 1024) {
            alert('File size must be less than 10MB');
            input.value = '';
            fileNameElement.textContent = '';
            return;
        }

        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
        if (!allowedTypes.includes(file.type)) {
            alert('Only image files (JPG, PNG, GIF) are allowed');
            input.value = '';
            fileNameElement.textContent = '';
            return;
        }

        fileNameElement.textContent = '📎 ' + file.name;
        fileNameElement.style.color = '#28a745';
        fileNameElement.style.fontWeight = '600';
        console.log('File validation passed');
    } else {
        fileNameElement.textContent = '';
    }
}

// Add form validation before submit
document.getElementById('createOrderForm').addEventListener('submit', function (e) {
    console.log('Form submission started');

    // Additional validation
    const fileInputs = this.querySelectorAll('input[type="file"]');

    for (let input of fileInputs) {
        if (input.files && input.files[0]) {
            if (input.files[0].size > 10 * 1024 * 1024) {
                e.preventDefault();
                alert('One or more files exceed the 10MB size limit');
                return;
            }
        }
    }

    console.log('Form validation passed, submitting...');
});