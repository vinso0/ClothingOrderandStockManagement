let packageCount = 1;
let packagesData = [];

function initializePackagesData(pkgs) {
    packagesData = pkgs || [];
}

function buildOptions() {
    let html = '<option value="">Select Package</option>';
    packagesData.forEach(p => {
        const price = Number(p.Price || 0);
        html += `<option value="${p.PackagesId}" data-price="${price}">${p.PackageName} - ₱${price.toFixed(2)}</option>`;
    });
    return html;
}

function addPackageRow() {
    const container = document.getElementById('packageContainer');
    const idx = packageCount++;
    const div = document.createElement('div');
    div.className = 'package-item';
    div.innerHTML = `
    <div class="row g-3">
      <div class="col-md-5">
        <label class="form-label">Package</label>
        <select class="form-select package-select" name="OrderPackages[${idx}].PackagesId" required>
          ${buildOptions()}
        </select>
      </div>
      <div class="col-md-3">
        <label class="form-label">Quantity</label>
        <input type="number" class="form-control quantity-input" name="OrderPackages[${idx}].Quantity" min="1" value="1" required />
      </div>
      <div class="col-md-3">
        <label class="form-label">Subtotal</label>
        <input type="text" class="form-control subtotal-display" id="subtotal-${idx}" readonly value="₱0.00" />
        <input type="hidden" name="OrderPackages[${idx}].PriceAtPurchase" id="price-${idx}" value="0" />
      </div>
      <div class="col-md-1 d-flex align-items-end">
        <button type="button" class="btn btn-remove" title="Remove package">×</button>
      </div>
    </div>
  `;
    container.appendChild(div);

    const select = div.querySelector('.package-select');
    const qty = div.querySelector('.quantity-input');
    const removeBtn = div.querySelector('.btn-remove');

    select.addEventListener('change', () => updatePrice(div, idx));
    qty.addEventListener('change', () => updatePrice(div, idx));
    removeBtn.addEventListener('click', () => { div.remove(); calculateTotal(); });

    // initialize calculation for the new row
    updatePrice(div, idx);
}

function updatePrice(row, index) {
    const select = row.querySelector('.package-select');
    const qtyInput = row.querySelector('.quantity-input');
    const priceHidden = row.querySelector(`#price-${index}`);
    const subtotalDisplay = row.querySelector(`#subtotal-${index}`);

    const qty = Math.max(1, parseInt(qtyInput.value || '1', 10));
    const price = Number(select.options[select.selectedIndex]?.getAttribute('data-price') || 0);
    const subtotal = price * qty;

    priceHidden.value = price.toFixed(2);
    subtotalDisplay.value = `₱${subtotal.toFixed(2)}`;
    calculateTotal();
}

function calculateTotal() {
    let total = 0;
    document.querySelectorAll('.subtotal-display').forEach(i => {
        total += Number((i.value || '₱0').replace(/[₱,]/g, '')) || 0;
    });
    const totalEl = document.getElementById('totalAmount');
    if (totalEl) totalEl.textContent = `₱${total.toFixed(2)}`;
}

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('createOrderForm');
    if (!form) return;

    // wire first existing row
    const firstRow = form.querySelector('.package-item');
    if (firstRow) {
        const select = firstRow.querySelector('.package-select');
        const qty = firstRow.querySelector('.quantity-input');
        const removeBtn = firstRow.querySelector('.btn-remove');
        if (select) select.addEventListener('change', () => updatePrice(firstRow, 0));
        if (qty) qty.addEventListener('change', () => updatePrice(firstRow, 0));
        if (removeBtn) removeBtn.addEventListener('click', () => { firstRow.remove(); calculateTotal(); });
        updatePrice(firstRow, 0);
    }

    // validate on submit
    form.addEventListener('submit', (e) => {
        const selects = form.querySelectorAll('.package-select');
        if (selects.length === 0) {
            e.preventDefault();
            alert('Please add at least one package.');
            return;
        }
        for (const s of selects) {
            if (!s.value) {
                e.preventDefault();
                alert('Please select a package for each row.');
                return;
            }
        }
    });
});

window.addPackageRow = addPackageRow; // expose to onclick
