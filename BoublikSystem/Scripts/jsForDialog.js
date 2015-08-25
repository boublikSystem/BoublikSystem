// JavaScript source code

// отображение модального окна для ссылок с класамми .compItem
function displayDialog () {
        $.ajaxSetup({ cache: false });
        $(".compItem").click(function (e) {

            e.preventDefault();
            $.get(this.href, function (data) {
                $('#dialogContent').html(data);
                $('#modDialog').modal('show');
            });
        });
}

//http://stackoverflow.com/questions/203198/event-binding-on-dynamically-created-elements
//$(".compItem").on("click", ".compItem", function() {
//    $.ajaxSetup({ cache: false });
//    $(".compItem").click(function (e) {

//        e.preventDefault();
//        $.get(this.href, function (data) {
//            $('#dialogContent').html(data);
//            $('#modDialog').modal('show');
//        });
//    });
//});

// изменяет количество полученный продуктов
function applyFunc() {
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

};


// проверка доступно ли заданное кличество товара
function checkCount() {
    var productId = document.getElementById("hiddenId").value;
    var availableCount = document.getElementById("productCountId" + productId);
    var inputCount = document.getElementById("countId").value;
    var submit = document.getElementById("applyId");
    var alerMsg = document.getElementById("alerMsg");

    availableCount = parseInt(availableCount.innerHTML, 10);
    //alert(availableCount.innerHTML);

    if (inputCount > availableCount) {
        submit.setAttribute("disabled", "");
        alerMsg.innerHTML = "Недостаточное количество продукта";

    } 
    else if (inputCount <= 0) {
        submit.setAttribute("disabled", "");
        alerMsg.innerHTML = "Введите правильное количество продукта";
    }
    else {
        submit.removeAttribute("disabled");
        alerMsg.innerHTML = "";
    }
}

// проверяем правильная ли сумма полученна
function textChanged() {
    var amountToPay = document.getElementById("amountToPay").innerHTML;
    var amoutToChange = document.getElementById("amoutToChange");
    var textId = document.getElementById("textId").value;

    amountToPay = parseInt(amountToPay);
    textId = textId.replace(",", ".");

    amountToChange.innerHTML = Math.round((textId - amountToPay) * 100) / 100;

    if (amountToChange.innerHTML >= 0) {
        var payed = document.getElementById("payed");

        payed.removeAttribute("disabled");
    }

}


// кнопка удаления из списка полученных товаров
function deleteFromBill () {
    var countId = document.getElementById("countAdded").innerHTML;
    var productId = document.getElementById("hiddenId").value;
    var elementToUpDate = "#update" + productId;

    $.ajax({
        type: "POST",
        url: "/sale/ChangeCount",
        success: function (result) {
            $(elementToUpDate).html(result);
        },
        error: function (xhr, status, error) {
            alert(xhr.responseText);
        },
        data: { productId: productId, productCount: countId, notNeeded: null }
    });

}

function payedFunc() {
    $("#modDialog").modal("hide");
}

function cancelFunc() {
    $("#modDialog").modal("hide");
}


   
    
