/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/jqueryui/jqueryui.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="common-library.ts" />
var EnteredCharts;
(function (EnteredCharts) {
    "use strict";
    var listing = false;
    var $listingLoading = $("#listingLoading").hide();
    $(document).ajaxStart(function () {
        if (listing === true) {
            $("#listingBody").empty();
            $listingLoading.show();
        }
    }).ajaxStop(function () {
        $listingLoading.hide();
        listing = false;
    });
    function GetEnteredChartsListing(page, numberPerPage) {
        listing = true;
        $.ajaxSetup({ cache: false });
        var jqxhr = $.getJSON("/EnteredCharts/GetEnteredChartsListing", {
            pageNum: page,
            entriesPerPage: numberPerPage
        }).done(function (data) {
            $.each(data[0], function (i, item) {
                var listingLine = $("<tr/>");
                listingLine.append($("<td/>").html(item.SubjectName));
                listingLine.append($("<td/>").html(item.SubjectLocation));
                listingLine.append($("<td/>").html(item.OriginDateTimeString));
                listingLine.append($("<td/>").html(item.ChartTypeName));
                listingLine.append($("<td/>").append($("<a href=\"#\" onclick=\"EnteredCharts.OpenEditForm(" + item.EnteredChartId + ");return false;\" title=\"Edit Entered Chart Data\"/>").append("<span class=\"fa fa-edit\"/>").append(" Edit")).append($("<br/>")).append($("<a href=\"/EnteredCharts/Details/" + item.EnteredChartId + "\" title=\"Details\"/>").append("<span class=\"fa fa-search\"/>").append(" Details")).append($("<br/>")).append($("<a href=\"/EnteredCharts/Transits/" + item.EnteredChartId + "\" title=\"Transits\"/>").append("<span class=\"fa fa-arrows-alt\"/>").append(" Transits")).append($("<br/>")).append($("<a href=\"/EnteredCharts/Synastry/" + item.EnteredChartId + "\" title=\"Synastry\"/>").append("<span class=\"fa fa-star-half-o\"/>").append(" Synastry")).append($("<br/>")).append($("<a href=\"#\" onclick=\"EnteredCharts.OpenDeleteForm(" + item.EnteredChartId + ");return false;\" title=\"Delete Entered Chart\"/>").append("<span class=\"fa fa-remove error\"/>").append($("<span class=\"error\"/>").html(" Delete"))));
                $("#listingBody").append(listingLine);
            });
            $("#pageNumber").empty();
            $("#totalPages").text(data[1]);
            for (var i = 1; i <= parseInt(data[1], 10); i++) {
                $("#pageNumber").append($((i === page) ? "<option selected=\"selected\"/>" : "<option/>").val(i.toString()).text(i.toString()));
            }
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Entered Charts listing failure: " + err);
            $("#status").html(errorIcon + " Error: Cannot retrieve Entered Charts Listing");
        });
        $.ajaxSetup({ cache: true });
        return jqxhr;
    }
    EnteredCharts.GetEnteredChartsListing = GetEnteredChartsListing;
    function OpenCreateForm() {
        $("#createEnteredChartModal").modal("show");
    }
    EnteredCharts.OpenCreateForm = OpenCreateForm;
    function OpenEditForm(id) {
        $("#editEnteredChartModal").modal("show");
        GetEditData(id);
    }
    EnteredCharts.OpenEditForm = OpenEditForm;
    function GetEditData(id) {
        // var jqxhr = 
        $.getJSON("/EnteredCharts/GetEnteredChartForEdit", { id: id }).done(function (data) {
            $("#editEnteredChartId").val(id.toString());
            $("#editSubjectName").val(data[0].SubjectName);
            $("#editSubjectLocation").val(data[0].SubjectLocation);
            // $("#editOriginDateTime").val((new Date(parseInt(data[0].OriginDateTime.substr(6), 10)).toJSON()));
            $("#editOriginDateTime").val(data[0].OriginDateTime);
            if (data[0].OriginDateTimeUnknown) {
                $("#editOriginDateTimeUnknown").prop("checked", true);
            }
            else {
                $("#editOriginDateTimeUnknown").prop("checked", false);
            }
            $("#editChartTypeId").val(data[0].ChartTypeId);
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Edit Load Failure: " + err);
            $("#editStatus").append($("<div/>").html(errorIcon + " Failed to get edit data."));
        });
    }
    EnteredCharts.GetEditData = GetEditData;
    function OpenDeleteForm(id) {
        $("#deleteEnteredChartModal").modal("show");
        // var jqxhr: JQueryPromise<any> = 
        $.getJSON("/EnteredCharts/GetEnteredChartForDelete", { id: id }).done(function (data) {
            $("#deleteEnteredChartId").val(data[0].EnteredChartId.toString());
            $("#deleteSubjectName").html(data[0].SubjectName);
            $("#deleteSubjectLocation").html(data[0].SubjectLocation);
            $("#deleteOriginDateTime").html(data[0].OriginDateTimeString);
            $("#deleteChartType").html(data[0].ChartTypeName);
            $("#deleteChartObjectsCount").html(data[0].NumberOfChartObjects.toString());
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Delete Load Failure: " + err);
            $("#status").append($("<div/>").html(errorIcon + " Failed to get delete data."));
        });
    }
    EnteredCharts.OpenDeleteForm = OpenDeleteForm;
    $("#confirmEnteredChartDelete").click(function () {
        // var jqxhr = 
        $.post("/EnteredCharts/ConfirmDeleteOfEnteredChart", {
            enteredChartId: $("#deleteEnteredChartId").val()
        }).done(function (data) {
            $("#status").html(successIcon + " Entered Chart deleted successfully.");
            $("#deleteEnteredChartModal").modal("hide");
            listing = true;
            $("#listingBody").empty();
            $listingLoading.show();
            $.when(GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10))).done(function () {
                $listingLoading.show();
                listing = false;
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Delete Load Failure: " + err);
            $("#deleteStatus").append($("<div/>").html(errorIcon + " Failed to delete Entered Chart."));
        });
    });
    $("#confirmEnteredChartEdit").click(function () {
        // var jqxhr = 
        $.post("/EnteredCharts/UpdateEnteredChart", {
            enteredChartId: $("#editEnteredChartId").val(),
            subjectName: $("#editSubjectName").val(),
            subjectLocation: $("#editSubjectLocation").val(),
            originDateTime: $("#editOriginDateTime").val(),
            originDateTimeUnknown: $("#editOriginDateTimeUnknown").is(":checked"),
            chartTypeId: $("#editChartTypeId").val()
        }).done(function (data) {
            if (data === "Success") {
                $("#status").html(successIcon + " Entered Chart updated successfully.");
                $("#editEnteredChartModal").modal("hide");
                listing = true;
                $("#listingBody").empty();
                $listingLoading.show();
                $.when(GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10))).done(function () {
                    $listingLoading.show();
                    listing = false;
                });
            }
            else {
                $("#editStatus").html(errorIcon + " Failed to update Entered Chart.<br />" + data);
            }
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Entered Chart Update Failure: " + err);
            $("#editStatus").html(errorIcon + " Failed to update Entered Chart.");
        });
    });
    $("#confirmEnteredChartCreate").click(function () {
        // var jqxhr = 
        $.post("/EnteredCharts/CreateNewEnteredChart", {
            subjectName: $("#createSubjectName").val(),
            subjectLocation: $("#createSubjectLocation").val(),
            originDateTime: $("#createOriginDateTime").val(),
            originDateTimeUnknown: $("#createOriginDateTimeUnknown").is(":checked"),
            chartTypeId: $("#createChartTypeId").val()
        }).done(function (data) {
            if (data === "Success") {
                $("#status").html(successIcon + " New Entered Chart created successfully.");
                $("#createEnteredChartModal").modal("hide");
                listing = true;
                $("#listingBody").empty();
                $listingLoading.show();
                $.when(GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10))).done(function () {
                    $listingLoading.show();
                    listing = false;
                });
            }
            else {
                $("#createStatus").html(errorIcon + " Failed to create new Entered Chart.<br />" + data);
            }
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Entered Chart Creation Failure: " + err);
            $("#createStatus").html(errorIcon + " Failed to create new Entered Chart.");
        });
    });
    function SetUpChartTypeDropdown() {
        $.getJSON("/EnteredCharts/GetChartTypesList").done(function (data) {
            $.each([$("#createChartTypeId"), $("#editChartTypeId")], function (i, list) {
                $.each(data, function (j, item) {
                    list.append($("<option value=\"" + item.ChartTypeId + "\"" + ((j === 0) ? " selected=\"selected\"" : "") + "/>").html(item.ChartTypeName));
                });
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Type List failure.: " + err);
            $("#status").append($("<div/>").html(errorIcon + " Failed to retrieve Chart Type List Types."));
        });
    }
    EnteredCharts.SetUpChartTypeDropdown = SetUpChartTypeDropdown;
    $("#resultsPerPage").change(function () {
        GetEnteredChartsListing(1, parseInt($("#resultsPerPage").val(), 10));
    });
    $("#pageNumber").change(function () {
        GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10));
    });
    $(document).ready(function () {
        GetEnteredChartsListing(1, 10);
        SetUpChartTypeDropdown();
    });
})(EnteredCharts || (EnteredCharts = {}));
//# sourceMappingURL=entered-charts.js.map