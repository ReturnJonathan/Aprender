// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/* 1) Estado oculto por defecto */
.music - player - footer {
    display: none;
    transition: all var(--transition - normal);
}

/* 2) Cuando queremos mostrarlo */
.music - player - footer.visible {
    display: block;
}

document.addEventListener('DOMContentLoaded', function () {
    const footer = document.querySelector('.music-player-footer');
    const audio = document.getElementById('footer-audio');
    const title = document.getElementById('footer-title');
    const artist = document.getElementById('footer-artist');
    const toggleBtn = document.getElementById('footer-toggle');
    const toggleIcon = document.getElementById('footer-toggle-icon');
    const songInfo = document.getElementById('footer-song-info');
    let isVisible = false;

    // 1) Al hacer click en cualquier botón “play-button”
    document.querySelectorAll('.play-button').forEach(btn => {
        btn.addEventListener('click', e => {
            e.preventDefault();
            const src = btn.dataset.audioSrc;
            const songTitle = btn.dataset.title;
            const songArtist = btn.dataset.artist;

            // Si ya se está mostrando esa misma canción, ocultamos el footer
            if (isVisible && audio.src.includes(src)) {
                footer.classList.remove('visible');
                isVisible = false;
                return;
            }

            // Cargamos los datos en el footer
            audio.src = src;
            title.textContent = songTitle;
            artist.textContent = songArtist;
            toggleIcon.classList.replace('fa-play', 'fa-pause');

            // Mostramos el footer y reproducimos
            footer.classList.add('visible');
            audio.play();
            isVisible = true;
        });
    });

    // 2) Toggle play/pause dentro del footer
    toggleBtn.addEventListener('click', () => {
        if (audio.paused) {
            audio.play();
            toggleIcon.classList.replace('fa-play', 'fa-pause');
        } else {
            audio.pause();
            toggleIcon.classList.replace('fa-pause', 'fa-play');
        }
    });

    // 3) Click en la info de la canción abre la vista completa
    songInfo.addEventListener('click', () => {
        let targetUrl = null;
        document.querySelectorAll('.play-button').forEach(b => {
            if (audio.src.includes(b.dataset.audioSrc)) {
                targetUrl = b.dataset.playerUrl;
            }
        });
        if (targetUrl) window.location.href = targetUrl;
    });
});
