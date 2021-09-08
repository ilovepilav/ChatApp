window.onload = () => {
    Particles.init({
        selector: '.background',
        connectParticles: true,
        maxParticles: 200,
        color: '#512d6d',
        responsive: [
            {
                breakpoint: 900,
                options: {
                    maxParticles: 0
                }
            }
        ]
    });
};
