// JavaScript source code

$("#applyId").click(function () {
    var countId = document.getElementById("countId").value;
    var productId = document.getElementById("hiddenId").value;
    var elementToUpDate = "#update" + productId;

    $.ajax({
        type: "GET",
        url: "/sale/ChangeCount",
        success: function (result) {
            $(elementToUpDate).html(result);
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);
        },
        data: { productId: productId, productCount: countId }
    });

    $("#modDialog").modal("hide");

});

function checkCount() {
    var productId = document.getElementById("hiddenId").value;
    var availableCount = document.getElementById("update" + productId);
    var inputCount = document.getElementById("countId").value;
    var submit = document.getElementById("applyId");
    var alerMsg = document.getElementById("alerMsg");

    availableCount = parseInt(availableCount.innerHTML, 10);

    if ((inputCount > availableCount)||(inputCount <= 0)||(inputCount.match("e"))) {
        submit.setAttribute("disabled", "");
        alerMsg.innerHTML = "Неправильно количество товара";
        
    } else {
        submit.removeAttribute("disabled");
        alerMsg.innerHTML = "";
    }
}

