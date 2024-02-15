// #region Variables
var qsCallerForm;
var qsEmpNo;
var _empNo;
// #endregion

$(function () {
    try {

        // Initialize variables
        gContainer = $('.formWrapper');

        // Get query string values
        qsCallerForm = GetQueryStringValue("callerForm");
        qsEmpNo = GetQueryStringValue("empNo");

        ShowLoadingPanel(gContainer, 1, 'Initalizing form, please wait...');

        // Initialize controls
        _empNo = GetIntValue($("#hidEmpNo").val());

        // Set the height of the report viewer based on the window size
        var height = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);
        $(".reportPanel").css("height", height - 150 + "px");
        //$(".reportPanel").css("height", "80vh");

        // Intialize event handlers
        $(".actionButton").on("click", handleActionButtonClick);
        $(window).resize(function () {
            var h = Math.max(document.documentElement.clientHeight, window.innerHeight || 0);
            $(".reportPanel").css("height", h - 150 + "px");
            //$(".reportPanel").css("height", "80vh");
        })

        // Move to the top of the page
        window.scrollTo(0, 0);

    } catch (err) {
        ShowErrorMessage("The following exception has occured while loading the page: " + err);
    }
    finally {
        HideLoadingPanel(gContainer);
    }
});

// #region Event Handlers
function handleActionButtonClick() {
    var btn = $(this);
    var hasError = false;
    var empNo = 0;

    // Hide all error messages
    HideErrorMessage();
    HideToastMessage();

    switch ($(btn)[0].id) {
        case "btnBack":
            if (qsCallerForm != "undefined" && qsCallerForm != null) {
                ShowLoadingPanel(gContainer, 1, 'Closing report, please wait...');
                location.href = qsCallerForm.concat("?contractorNo=").concat(qsEmpNo).concat("&isback=1");
            }
            break;
    }
}


// #endregion