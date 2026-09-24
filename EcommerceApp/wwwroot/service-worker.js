// ============================================================
// NEXHARD - Service Worker (PWA + Push Notifications)
// ============================================================

const CACHE_NAME = 'nexhard-v1';
const urlsToCache = [
    '/',
    '/css/white-tech.css',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/js/site.js'
];

// INSTALACIÓN: cachear recursos básicos
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});

// ACTIVACIÓN: limpiar cachés viejos
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

// FETCH: servir desde caché si está offline
self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => response || fetch(event.request))
    );
});

// ============================================================
// PUSH NOTIFICATIONS
// ============================================================
self.addEventListener('push', event => {
    if (!event.data) {
        console.log('Push sin datos');
        return;
    }

    const data = event.data.json();

    const options = {
        body: data.body || 'Nueva notificación de NexHard',
        icon: data.icon || '/images/logo-nexhard.png',
        badge: data.badge || '/images/logo-nexhard.png',
        image: data.image,
        data: data.data || { url: '/' },
        tag: data.tag || 'nexhard-general',
        requireInteraction: false,
        silent: false,
        vibrate: [200, 100, 200]
    };

    event.waitUntil(
        self.registration.showNotification(data.title || 'NexHard', options)
    );
});

// Al hacer clic en la notificación
self.addEventListener('notificationclick', event => {
    event.notification.close();

    const url = event.notification.data?.url || '/';

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true })
            .then(windowClients => {
                // Si ya hay una ventana abierta, enfocarla
                for (let client of windowClients) {
                    if (client.url === url && 'focus' in client) {
                        return client.focus();
                    }
                }
                // Si no, abrir una nueva
                if (clients.openWindow) {
                    return clients.openWindow(url);
                }
            })
    );
});