/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/jqueryui/jqueryui.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="common-library.ts" />

module EnteredCharts {
    "use strict";

    var listing: boolean = false;

    var $listingLoading = $("#listingLoading").hide();
    $(document)
        .ajaxStart(function () {
        if (listing === true) {
            $("#listingBody").empty();
            $listingLoading.show();
        }
    })
        .ajaxStop(function () {
        $listingLoading.hide();
        listing = false;
    });

    export function GetEnteredChartsListing(page: number, numberPerPage: number) {
        listing = true;

        $.ajaxSetup({ cache: false });

        var jqxhr = $.getJSON("/EnteredCharts/GetEnteredChartsListing", {
            pageNum: page,
            entriesPerPage: numberPerPage
        }).done(function (data: any) {
            $.each(data[0], function (i: any, item: any) {
                var listingLine = $("<tr/>");

                listingLine.append($("<td/>").html(item.SubjectName));

                listingLine.append($("<td/>").html(item.SubjectLocation));

                listingLine.append($("<td/>").html(item.OriginDateTimeString));

                listingLine.append($("<td/>").html(item.ChartTypeName));

                listingLine
                    .append(
                    $("<td/>")
                        .append(
                        $("<a href=\"#\" onclick=\"EnteredCharts.OpenEditForm("
                            + item.EnteredChartId
                            + ");return false;\" title=\"Edit Entered Chart Data\"/>")
                            .append("<span class=\"fa fa-edit\"/>")
                            .append(" Edit")
                        )
                        .append($("<br/>"))
                        .append(
                        $("<a href=\"/EnteredCharts/Details/" + item.EnteredChartId + "\" title=\"Details\"/>")
                            .append("<span class=\"fa fa-search\"/>")
                            .append(" Details")
                        )
                        .append($("<br/>"))
                        .append(
                        $("<a href=\"/EnteredCharts/Transits/" + item.EnteredChartId + "\" title=\"Transits\"/>")
                            .append("<span class=\"fa fa-arrows-alt\"/>")
                            .append(" Transits")
                        )
                        .append($("<br/>"))
                        .append(
                        $("<a href=\"/EnteredCharts/Synastry/" + item.EnteredChartId + "\" title=\"Synastry\"/>")
                            .append("<span class=\"fa fa-star-half-o\"/>")
                            .append(" Synastry")
                        )
                        .append($("<br/>"))
                        .append(
                        $("<a href=\"#\" onclick=\"EnteredCharts.OpenDeleteForm("
                            + item.EnteredChartId
                            + ");return false;\" title=\"Delete Entered Chart\"/>")
                            .append("<span class=\"fa fa-remove error\"/>")
                            .append($("<span class=\"error\"/>").html(" Delete")
                            )
                        )
                    );

                $("#listingBody").append(listingLine);
            });

            $("#pageNumber").empty();
            $("#totalPages").text(data[1]);
            for (var i = 1; i <= parseInt(data[1], 10); i++) {
                $("#pageNumber").append($((i === page) ? "<option selected=\"selected\"/>" : "<option/>")
                    .val(i.toString()).text(i.toString()));
            }
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
            var err = textStatus + ", " + error;
            console.log("Entered Charts listing failure: " + err);
            $("#status").html(errorIcon + " Error: Cannot retrieve Entered Charts Listing");
        });

        $.ajaxSetup({ cache: true });

        return jqxhr;
    }

    export function OpenCreateForm() {
        $("#createEnteredChartModal").modal("show");
    }

    export function OpenEditForm(id: number) {
        $("#editEnteredChartModal").modal("show");
        GetEditData(id);
    }

    export function GetEditData(id: number) {
        // var jqxhr = 
        $.getJSON("/EnteredCharts/GetEnteredChartForEdit", { id: id }).done(function (data: any) {
            $("#editEnteredChartId").val(id.toString());
            $("#editSubjectName").val(data[0].SubjectName);
            $("#editSubjectLocation").val(data[0].SubjectLocation);
            // $("#editOriginDateTime").val((new Date(parseInt(data[0].OriginDateTime.substr(6), 10)).toJSON()));
            $("#editOriginDateTime").val(data[0].OriginDateTime);
            if (data[0].OriginDateTimeUnknown) {
                $("#editOriginDateTimeUnknown").prop("checked", true);
            } else {
                $("#editOriginDateTimeUnknown").prop("checked", false);
            }
            $("#editChartTypeId").val(data[0].ChartTypeId);
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
            var err = textStatus + ", " + error;
            console.log("Edit Load Failure: " + err);
            $("#editStatus").append($("<div/>").html(errorIcon + " Failed to get edit data."));
        });
    }

    export function OpenDeleteForm(id: number) {
        $("#deleteEnteredChartModal").modal("show");

        // var jqxhr: JQueryPromise<any> = 
        $.getJSON("/EnteredCharts/GetEnteredChartForDelete", { id: id }).done(function (data: any) {
            $("#deleteEnteredChartId").val(data[0].EnteredChartId.toString());
            $("#deleteSubjectName").html(data[0].SubjectName);
            $("#deleteSubjectLocation").html(data[0].SubjectLocation);
            $("#deleteOriginDateTime").html(data[0].OriginDateTimeString);
            $("#deleteChartType").html(data[0].ChartTypeName);
            $("#deleteChartObjectsCount").html(data[0].NumberOfChartObjects.toString());
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
            var err = textStatus + ", " + error;
            console.log("Delete Load Failure: " + err);
            $("#status").append($("<div/>").html(errorIcon + " Failed to get delete data."));
        });
    }

    $("#confirmEnteredChartDelete").click(function () {
        // var jqxhr = 
        $.post("/EnteredCharts/ConfirmDeleteOfEnteredChart", {
            enteredChartId: $("#deleteEnteredChartId").val()
        }).done(function (data: any) {
            $("#status").html(successIcon + " Entered Chart deleted successfully.");

            $("#deleteEnteredChartModal").modal("hide");

            listing = true;
            $("#listingBody").empty();
            $listingLoading.show();
            $.when(GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10)))
                .done(function () {
                $listingLoading.show();
                listing = false;
            });
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
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
        }).done(function (data: any) {
            if (data === "Success") {
                $("#status").html(successIcon + " Entered Chart updated successfully.");

                $("#editEnteredChartModal").modal("hide");

                listing = true;
                $("#listingBody").empty();
                $listingLoading.show();
                $.when(GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10)))
                    .done(function () {
                    $listingLoading.show();
                    listing = false;
                });
            } else {
                $("#editStatus").html(errorIcon + " Failed to update Entered Chart.<br />" + data);
            }
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
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
        }).done(function (data: any) {
            if (data === "Success") {
                $("#status").html(successIcon + " New Entered Chart created successfully.");

                $("#createEnteredChartModal").modal("hide");

                listing = true;
                $("#listingBody").empty();
                $listingLoading.show();
                $.when(GetEnteredChartsListing(parseInt($("#pageNumber").val(), 10), parseInt($("#resultsPerPage").val(), 10)))
                    .done(function () {
                    $listingLoading.show();
                    listing = false;
                });
            } else {
                $("#createStatus").html(errorIcon + " Failed to create new Entered Chart.<br />" + data);
            }
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
            var err = textStatus + ", " + error;
            console.log("Entered Chart Creation Failure: " + err);
            $("#createStatus").html(errorIcon + " Failed to create new Entered Chart.");
        });
    });

    export function SetUpChartTypeDropdown() {
        $.getJSON("/EnteredCharts/GetChartTypesList").done(function (data: any) {
            $.each([$("#createChartTypeId"), $("#editChartTypeId")], function (i: any, list: any) {
                $.each(data, function (j: number, item: any) {
                    list.append($("<option value=\"" + item.ChartTypeId + "\"" + ((j === 0) ? " selected=\"selected\"" : "") + "/>")
                        .html(item.ChartTypeName));
                });
            });
        }).fail(function (jqxhr: any, textStatus: any, error: any) {
            var err = textStatus + ", " + error;
            console.log("Chart Type List failure.: " + err);
            $("#status").append($("<div/>").html(errorIcon + " Failed to retrieve Chart Type List Types."));
        });
    }

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
}