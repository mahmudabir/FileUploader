// File: src/service-worker.js
self.addEventListener('fetch', (event) => {
    const requestUrl = new URL(event.request.url);
  
    if (requestUrl.pathname.startsWith('/api/video')) {
      event.respondWith(
        caches.open('video-cache').then((cache) => {
          return cache.match(event.request).then((response) => {
            return (
              response ||
              fetch(event.request).then((networkResponse) => {
                cache.put(event.request, networkResponse.clone());
                return networkResponse;
              })
            );
          });
        })
      );
    }
  });
  