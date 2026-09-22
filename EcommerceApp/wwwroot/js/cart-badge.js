// ============================================
// NEXHARD - CARRITO: Contador dinámico + Toasts
// ============================================

// Actualizar el contador del carrito al cargar la página
document.addEventListener('DOMContentLoaded', function () {
    updateCartBadge();
});

function updateCartBadge() {
    fetch('/Cart/GetCount')
        .then(response => response.json())
        .then(data => {
            const badge = document.getElementById('cartBadge');
            if (badge) {
                badge.textContent = data.count;
                if (data.count > 0) {
                    badge.style.display = 'flex';
                } else {
                    badge.style.display = 'none';
                }
            }
        })
        .catch(err => console.log('Error al actualizar carrito:', err));
}

// ============================================
// Sistema de notificaciones (toast)
// ============================================
function showToast(message, type = 'success') {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = `
            position: fixed;
            top: 90px;
            right: 20px;
            z-index: 9999;
            display: flex;
            flex-direction: column;
            gap: 10px;
            pointer-events: none;
        `;
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    const bgColor = type === 'success'
        ? 'linear-gradient(90deg, #00e0ff, #a020f0)'
        : '#ff4444';
    const icon = type === 'success' ? '✅' : '⚠️';

    toast.style.cssText = `
        background: ${bgColor};
        color: white;
        padding: 14px 20px;
        border-radius: 12px;
        font-weight: 600;
        font-size: 0.9rem;
        box-shadow: 0 10px 30px rgba(0, 224, 255, 0.4);
        display: flex;
        align-items: center;
        gap: 10px;
        animation: slideInRight 0.4s ease-out;
        pointer-events: auto;
        max-width: 320px;
    `;
    toast.innerHTML = `<span style="font-size:1.2rem;">${icon}</span> <span>${message}</span>`;

    container.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideOutRight 0.4s ease-in';
        setTimeout(() => toast.remove(), 400);
    }, 3000);
}