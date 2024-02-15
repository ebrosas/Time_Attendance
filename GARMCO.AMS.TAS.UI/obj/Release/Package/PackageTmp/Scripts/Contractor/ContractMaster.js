// #region Global Variables 
var urlForms = {
    ContractorRegistration: "RegisterContractor.aspx",
    ContractorInquiry: "ContractorInquiry.aspx",
    IDCardGenerator: "IDCardGenerator.aspx"
};

var formTypes = {
    ContractorRegistration: "register",
    ContractorInquiry: "inquiry",
    IDCardGenerator: "idcard"
};
// #endregion

$(function () {
    // Set the current container
    gContainer = $('.formWrapper');

    // Initialize the height of the iFrame based on the browser window height
    var height = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);
    $(".ifInnerFrame").css("height", height - 150 + "px");
    //$(".ifInnerFrame").css("height", "85vh");

    var formType = $("input[id$=hidFormName").val();
    switch (formType) {
        case formTypes.ContractorRegistration:
            $('.ifInnerFrame').attr("src", urlForms.ContractorRegistration);
            break;

        case formTypes.ContractorInquiry:
            $('.ifInnerFrame').attr("src", urlForms.ContractorInquiry);
            break;

        case formTypes.IDCardGenerator:
            $('.ifInnerFrame').attr("src", urlForms.IDCardGenerator);
            break;
    }

    HideLoadingPanel(gContainer);
});

// #region Private Functions
function showLoadingPanel() {
    ShowLoadingPanel($('.formWrapper'), 1, 'Please wait...');
}
// #endregion