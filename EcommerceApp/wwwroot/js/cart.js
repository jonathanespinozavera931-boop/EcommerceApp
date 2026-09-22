// ============================================
// NEXHARD - LÓGICA DEL CARRITO (Página Cart/Index)
// ============================================

// Obtener el token anti-forgery del formulario
function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : '';
}

// ============================================
// CAMBIAR CANTIDAD (+ o -)
// ============================================
document.querySelectorAll('.qty-btn').forEach(btn => {
    btn.addEventListener('click', function () {
        const productId = this.dataset.id;
        const action = this.dataset.action;
        const qtySpan = document.getElementById(`qty-${productId}`);
        let currentQty = parseInt(qtySpan.textContent);

        let newQty = action === 'plus' ? currentQty + 1 : currentQty - 1;

        if (newQty < 1) {
            // Preguntar si quiere eliminar
            if (confirm('¿Eliminar este producto del carrito?')) {
                removeItem(productId);
            }
            return;
        }

        updateQuantity(productId, newQty);
    });
});

// ============================================
// ACTUALIZAR CANTIDAD EN EL SERVIDOR
// ============================================
function updateQuantity(productId, quantity) {
    const token = getAntiForgeryToken();
    const formData = new FormData();
    formData.append('productId', productId);
    formData.append('quantity', quantity);
    formData.append('__RequestVerificationToken', token);

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Actualizar cantidad mostrada
                document.getElementById(`qty-${productId}`).textContent = quantity;

                // Actualizar subtotal del item
                document.getElementById(`subtotal-${productId}`).textContent =
                    'Bs ' + data.itemSubtotal.toFixed(2);

                // Actualizar resumen
                updateSummary(data.subtotal);

                // Actualizar badge del navbar
                if (typeof updateCartBadge === 'function') {
                    updateCartBadge();
                }
            } else {
                if (typeof showToast === 'function') {
                    showToast(data.message, 'error');
                } else {
                    alert(data.message);
                }
            }
        })
        .catch(err => {
            console.error('Error:', err);
            alert('Error al actualizar la cantidad');
        });
}

// ============================================
// ELIMINAR PRODUCTO
// ============================================
document.querySelectorAll('.cart-item-remove').forEach(btn => {
    btn.addEventListener('click', function () {
        const productId = this.dataset.id;
        if (confirm('¿Eliminar este producto del carrito?')) {
            removeItem(productId);
        }
    });
});

function removeItem(productId) {
    const token = getAntiForgeryToken();
    const formData = new FormData();
    formData.append('productId', productId);
    formData.append('__RequestVerificationToken', token);

    fetch('/Cart/Remove', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Animación de salida
                const item = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
                if (item) {
                    item.style.transition = 'all 0.3s';
                    item.style.opacity = '0';
                    item.style.transform = 'translateX(-30px)';
                    setTimeout(() => {
                        item.remove();
                        // Si ya no hay items, recargar para mostrar el estado vacío
                        if (document.querySelectorAll('.cart-item').length === 0) {
                            location.reload();
                        }
                    }, 300);
                }

                // Actualizar resumen
                updateSummary(data.subtotal);

                // Actualizar badge
                if (typeof updateCartBadge === 'function') {
                    updateCartBadge();
                }
            }
        })
        .catch(err => {
            console.error('Error:', err);
            alert('Error al eliminar el producto');
        });
}

// ============================================
// ACTUALIZAR RESUMEN (Subtotal, Envío, Total)
// ============================================
function updateSummary(subtotal) {
    const ENVIO_GRATIS_DESDE = 500;
    const ENVIO_COSTO = 20;

    let envio = 0;
    let envioTexto = '';

    if (subtotal >= ENVIO_GRATIS_DESDE) {
        envio = 0;
        envioTexto = '<span class="envio-gratis">🎉 ¡GRATIS!</span>';
    } else if (subtotal > 0) {
        envio = ENVIO_COSTO;
        envioTexto = 'Bs ' + envio.toFixed(2);
    } else {
        envio = 0;
        envioTexto = 'Bs 0.00';
    }

    const total = subtotal + envio;

    // Actualizar los elementos del DOM
    const subtotalEl = document.getElementById('summary-subtotal');
    const envioEl = document.getElementById('summary-envio');
    const totalEl = document.getElementById('summary-total');

    if (subtotalEl) subtotalEl.textContent = 'Bs ' + subtotal.toFixed(2);
    if (envioEl) envioEl.innerHTML = envioTexto;
    if (totalEl) totalEl.textContent = 'Bs ' + total.toFixed(2);

    // Actualizar el mensaje de "te faltan X para envío gratis"
    const envioInfo = document.querySelector('.envio-info');
    if (subtotal > 0 && subtotal < ENVIO_GRATIS_DESDE) {
        const faltan = (ENVIO_GRATIS_DESDE - subtotal).toFixed(2);
        if (envioInfo) {
            envioInfo.innerHTML = `🚚 Te faltan <strong>Bs ${faltan}</strong> para envío gratis`;
        }
    } else if (envioInfo) {
        envioInfo.remove();
    }
}