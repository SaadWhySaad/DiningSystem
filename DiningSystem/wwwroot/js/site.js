// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



/* protected void btnLogin_Click(object sender, EventArgs e)
{
	Response.Redirect("~/Restaurant");
}*/




function levenshtein(a, b) {
    const an = a ? a.length : 0;
    const bn = b ? b.length : 0;
    if (an === 0) return bn;
    if (bn === 0) return an;

    const matrix = Array(an + 1).fill(null).map(() => Array(bn + 1).fill(null));
    for (let i = 0; i <= an; i += 1) matrix[i][0] = i;
    for (let j = 0; j <= bn; j += 1) matrix[0][j] = j;

    for (let i = 0; i < an; i += 1) {
        for (let j = 0; j < bn; j += 1) {
            const cost = a[i] === b[j] ? 0 : 1;
            matrix[i + 1][j + 1] = Math.min(
                matrix[i][j + 1] + 1,
                matrix[i + 1][j] + 1,
                matrix[i][j] + cost,
            );
        }
    }
    return matrix[an][bn];
}


function searchContent() {
    var searchText = document.getElementById("searchInput").value.toLowerCase();
    var restaurantCards = document.querySelectorAll(".box");
    var threshold = 3; // Maximum distance for fuzzy matching

    restaurantCards.forEach(function (card) {
        var cardTitle = card.querySelector(".box-title").textContent.toLowerCase();
        var distance = levenshtein(searchText, cardTitle);

        if (cardTitle.includes(searchText) || distance <= threshold) {
            card.style.display = "block";
        } else {
            card.style.display = "none";
        }
    });
}





/*document.addEventListener("DOMContentLoaded", function () {
    var loaderText = document.getElementById("loader-text");
    var words = "Experience The Taste Of World".split(" ");
    var index = 0;

    function showNextWord() {
        if (index < words.length) {
            loaderText.style.opacity = 0;
            setTimeout(function () {
                loaderText.textContent = words[index];
                loaderText.style.opacity = 1;
                index++;
                setTimeout(showNextWord, 300); // Delay before the next word appears
            }, 400); // Time for the word to disappear
        } else {
            setTimeout(function () {
                document.getElementById("loader").style.top = "-100%";
                document.getElementById("content").style.display = "block";
            }, 1000);
        }
    }

    showNextWord();
});*/

document.addEventListener("DOMContentLoaded", function () {
    var loaderText = document.getElementById("loader-text");
    var words = "Experience The Taste Of World".split(" ");
    var index = 0;
    var welcomeSound = document.getElementById("welcomeSound");
    // Check if the loader has already been shown in this session
    if (sessionStorage.getItem('loaderShown') === 'true') {
        document.getElementById("loader").style.display = "none";
        document.getElementById("content").style.display = "block";
        return; // Don't show the loader again
    } else {
        sessionStorage.setItem('loaderShown', 'true'); // Set the flag
    }

    function showNextWord() {
        if (index < words.length) {
            loaderText.style.opacity = 0;
            setTimeout(function () {
                loaderText.textContent = words[index];
                loaderText.style.opacity = 1;
                index++;
                setTimeout(showNextWord, 300); // Delay before the next word appears
            }, 400); // Time for the word to disappear
        } else {
            setTimeout(function () {
                document.getElementById("loader").style.top = "-100%";
                document.getElementById("content").style.display = "block";
                playWelcomeSound(); // Play the welcome sound
            }, 500);
        }
    }

    function createFloatingSpoons() {
        var background = document.getElementById("loader-background");
        for (var i = 0; i < 20; i++) { // Adjust the number of spoons
            var spoon = document.createElement("div");
            spoon.className = "floating-spoon";
            spoon.style.left = Math.random() * 100 + "vw"; // Random horizontal position
            spoon.style.top = Math.random() * 100 + "vh";  // Random vertical position
            spoon.style.animationDelay = Math.random() * 5 + "s"; // Stagger animation start
            background.appendChild(spoon);
        }
    }

    createFloatingSpoons();
    showNextWord();
});

document.addEventListener('DOMContentLoaded', function () {
    const cards = document.querySelectorAll('.card');

    const observerOptions = {
        root: null,
        rootMargin: '0px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    cards.forEach(card => {
        observer.observe(card);
    });
});
