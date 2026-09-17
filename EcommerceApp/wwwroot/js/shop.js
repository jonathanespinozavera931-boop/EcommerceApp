document.addEventListener('DOMContentLoaded', function () {
    const grid = document.getElementById('productGrid');
    if (!grid) return;
    const cards = Array.from(grid.querySelectorAll('.product-card'));

    // --- Favoritos (localStorage) ---
    const favs = JSON.parse(localStorage.getItem('nexhard_favs') || '[]');
    document.querySelectorAll('.pc-fav').forEach(btn => {
        const id = btn.dataset.id;
        if (favs.includes(id)) { btn.classList.add('active'); btn.textContent = '♥'; }
        btn.addEventListener('click', () => {
            let list = JSON.parse(localStorage.getItem('nexhard_favs') || '[]');
            if (list.includes(id)) {
                list = list.filter(x => x !== id);
                btn.classList.remove('active'); btn.textContent = '♡';
            } else {
                list.push(id);
                btn.classList.add('active'); btn.textContent = '♥';
            }
            localStorage.setItem('nexhard_favs', JSON.stringify(list));
        });
    });

    // --- Cantidad +/- ---
    document.querySelectorAll('.qty-row').forEach(row => {
        const valEl = row.querySelector('.qty-val');
        row.querySelector('.qty-minus').addEventListener('click', () => {
            let v = parseInt(valEl.textContent); if (v > 1) valEl.textContent = v - 1;
        });
        row.querySelector('.qty-plus').addEventListener('click', () => {
            valEl.textContent = parseInt(valEl.textContent) + 1;
        });
    });

    // --- Carrito (localStorage) ---
    function updateCartBadge() {
        const cart = JSON.parse(localStorage.getItem('nexhard_cart') || '[]');
        const total = cart.reduce((s, i) => s + i.qty, 0);
        const badge = document.getElementById('cartBadge');
        if (badge) badge.textContent = total;
    }
    document.querySelectorAll('.btn-cart').forEach(btn => {
        btn.addEventListener('click', () => {
            const card = btn.closest('.product-card');
            const qty = parseInt(card.querySelector('.qty-val')?.textContent || '1');
            const cart = JSON.parse(localStorage.getItem('nexhard_cart') || '[]');
            const existing = cart.find(i => i.id === card.dataset.id);
            if (existing) existing.qty += qty; else cart.push({ id: card.dataset.id, name: card.dataset.name, qty });
            localStorage.setItem('nexhard_cart', JSON.stringify(cart));
            updateCartBadge();
            btn.classList.add('added');
            const original = btn.textContent;
            btn.textContent = '✓ Agregado';
            setTimeout(() => { btn.classList.remove('added'); btn.textContent = original; }, 1200);
        });
    });
    updateCartBadge();

    // --- Búsqueda ---
    const search = document.getElementById('searchInput');
    if (search) search.addEventListener('input', applyFilters);

    // --- Filtro por categoría ---
    document.querySelectorAll('.filter-chip').forEach(chip => {
        chip.addEventListener('click', () => {
            document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
            chip.classList.add('active');
            applyFilters();
        });
    });

    document.getElementById('clearFilters')?.addEventListener('click', () => {
        document.querySelectorAll('.filter-chip').forEach(c => c.classList.remove('active'));
        document.querySelector('.filter-chip[data-cat="all"]').classList.add('active');
        if (search) search.value = '';
        applyFilters();
    });

    function applyFilters() {
        const term = (search?.value || '').toLowerCase();
        const activeCat = document.querySelector('.filter-chip.active')?.dataset.cat || 'all';
        cards.forEach(card => {
            const matchesCat = activeCat === 'all' || card.dataset.category === activeCat;
            const matchesTerm = card.dataset.name.toLowerCase().includes(term);
            card.style.display = (matchesCat && matchesTerm) ? '' : 'none';
        });
    }

    // --- Ordenar ---
    const sortSelect = document.getElementById('sortSelect');
    if (sortSelect) sortSelect.addEventListener('change', () => {
        const val = sortSelect.value;
        const sorted = [...cards].sort((a, b) => {
            if (val === 'price-asc') return parseFloat(a.dataset.price) - parseFloat(b.dataset.price);
            if (val === 'price-desc') return parseFloat(b.dataset.price) - parseFloat(a.dataset.price);
            if (val === 'name') return a.dataset.name.localeCompare(b.dataset.name);
            return 0;
        });
        sorted.forEach(c => grid.appendChild(c));
    });

    // --- Vista rápida ---
    const overlay = document.getElementById('quickViewOverlay');
    document.querySelectorAll('.pc-quick').forEach(btn => {
        btn.addEventListener('click', () => {
            const card = btn.closest('.product-card');
            document.getElementById('qvName').textContent = card.dataset.name;
            document.getElementById('qvCategory').textContent = card.dataset.category;
            document.getElementById('qvDesc').textContent = card.dataset.desc;
            document.getElementById('qvPrice').textContent = 'Bs ' + parseFloat(card.dataset.price).toFixed(2);
            document.getElementById('qvStock').textContent = card.dataset.stock;
            const img = document.getElementById('qvImage');
            img.className = 'qv-image ' + (card.dataset.imgClass || '');
            img.style.backgroundImage = card.dataset.imgUrl ? `url('${card.dataset.imgUrl}')` : '';
            img.textContent = card.dataset.imgUrl ? '' : card.dataset.icon;
            overlay.classList.add('open');
        });
    });
    document.getElementById('qvClose')?.addEventListener('click', () => overlay.classList.remove('open'));
    overlay?.addEventListener('click', e => { if (e.target === overlay) overlay.classList.remove('open'); });

    // --- Filtros en móvil ---
    document.getElementById('mobileFilterBtn')?.addEventListener('click', () => {
        document.getElementById('filterSidebar').classList.add('open');
    });
    document.getElementById('closeMobileFilter')?.addEventListener('click', () => {
        document.getElementById('filterSidebar').classList.remove('open');
    });
});