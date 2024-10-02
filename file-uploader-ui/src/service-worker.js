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
  


  // Plyr multiple quality implementation
  /*
  <!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/plyr/3.7.8/plyr.css"
        integrity="sha512-yexU9hwne3MaLL2PG+YJDhaySS9NWcj6z7MvUDSoMhwNghPgXgcvYgVhfj4FMYpPh1Of7bt8/RK5A0rQ9fPMOw=="
        crossorigin="anonymous" referrerpolicy="no-referrer" />
    <style>
        .container {
            margin: 20px auto;
            width: 600px;
        }

        video {
            width: 100%;
        }
    </style>
</head>

<body>

    <script src="https://cdn.jsdelivr.net/npm/hls.js"></script>
    <script src="https://unpkg.com/plyr@3"></script>
    <script>
        document.addEventListener("DOMContentLoaded", () => {
            const video = document.querySelector("video");
            const source = video.getElementsByTagName("source")[0].src;

            // For more options see: https://github.com/sampotts/plyr/#options
            // captions.update is required for captions to work with hls.js
            const defaultOptions = {};

            if (Hls.isSupported()) {
                // For more Hls.js options, see https://github.com/dailymotion/hls.js
                const hls = new Hls();
                hls.loadSource(source);

                // From the m3u8 playlist, hls parses the manifest and returns
                // all available video qualities. This is important, in this approach,
                // we will have one source on the Plyr player.
                hls.on(Hls.Events.MANIFEST_PARSED, function (event, data) {

                    // Transform available levels into an array of integers (width values).
                    const availableQualities = hls.levels.map((l) => l.width)

                    // Add new qualities to option
                    defaultOptions.quality = {
                        default: availableQualities[0],
                        options: availableQualities,
                        // this ensures Plyr to use Hls to update quality level
                        forced: true,
                        onChange: (e) => updateQuality(e),
                    }

                    // Initialize here
                    const player = new Plyr(video, defaultOptions);
                });
                hls.attachMedia(video);
                window.hls = hls;
            } else {
                // default options with no quality update in case Hls is not supported
                const player = new Plyr(video, defaultOptions);
            }

            function updateQuality(newQuality) {
                window.hls.levels.forEach((level, levelIndex) => {
                    if (level.height === newQuality) {
                        console.log("Found quality match with " + newQuality);
                        window.hls.currentLevel = levelIndex;
                    }
                });
            }
        });

    </script>

    <div class="container">
        <video controls crossorigin playsinline>
            <source type="application/x-mpegURL" src="https://localhost:7001/api/Streaming/master.m3u8?file=inputs.mp4">
        </video>
    </div>

</body>

</html>
  */