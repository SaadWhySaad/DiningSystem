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





