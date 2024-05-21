// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



/* protected void btnLogin_Click(object sender, EventArgs e)
{
	Response.Redirect("~/Restaurant");
}*/

function searchContent() {
    var searchText = document.getElementById("searchInput").value.toLowerCase();
    var restaurantCards = document.querySelectorAll(".box");

    restaurantCards.forEach(function (card) {
        var cardText = card.textContent.toLowerCase();
        if (cardText.includes(searchText)) {
            card.style.display = "block"; 
        } else {
            card.style.display = "none"; 
        }
    });
}

