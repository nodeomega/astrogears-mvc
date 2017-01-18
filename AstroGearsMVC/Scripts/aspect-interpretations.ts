/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/jqueryui/jqueryui.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="common-library.ts" />

module AspectInterpretations {
    var listing = false;

    $(".celestial-object").autocomplete({
        source: (request, response) => {
            $.getJSON('/AspectInterpretations/GetFullAutoComplete', { enteredTerm: request.term })
                .done(function(data) {
                    response($.map(data,
                        function(item) {
                            return {
                                id: item.CelestialObjectId,
                                label: item.CelestialObjectName,
                                value: item.CelestialObjectName
                            };
                        }));
                })
                .fail(function(ex) {
                    console.log("AutoComplete Failed.");
                });
        },
        minLength: 2,
        select: function (event, ui) {
            //console.log(ui.item ?
            //"Selected: " + ui.item.value + " aka " + ui.item.id + " and exists " + ui.item.exists :
            //"Nothing selected, input was " + this.value);
            var thisId = '#' + $(this).attr('id') + 'Id';
            var thisSelected = '#' + $(this).attr('id') + 'Selected';

            if (ui.item) {
                $(thisId).val(ui.item.id);
                $(thisSelected).val(ui.item.value);
            } else {
                $(thisId).val('');
                $(thisSelected).val($(this).val());
            }
        }
    });

    var $listingLoading = $('#listingLoading').hide();
    $(document)
        .ajaxStart(function () {
            if (listing === true) {
                $('#listingBody').empty();
                $listingLoading.show();
            }
        })
        .ajaxStop(function () {
            $listingLoading.hide();
            listing = false;
        });

    export function GetAspectInterpretationListing(page, numberPerPage) {
        listing = true;

        $.ajaxSetup({ cache: false });

        var jqxhr = $.getJSON('/AspectInterpretations/GetAspectInterpretationListing', {
            pageNum: page,
            entriesPerPage: numberPerPage
        }).done(function (data) {
            $.each(data[0], function (i, item) {
                var listingLine = $('<tr/>');

                listingLine.append($('<td/>').html(item.FirstObject + ' ' + item.AspectName + ' ' + item.SecondObject));

                listingLine.append($('<td/>').html(item.Interpretation.replace(/\n/g, '<br>')));

                listingLine.append($('<td/>').html((!IsNullOrUndefined(item.CitationUrl) && item.CitationUrl.substring(0, 4) === 'http'
                    ? '<a href="' + item.CitationUrl + '" target="_blank">' + item.CitationUrl + '</a>'
                    : item.CitationUrl)));

                listingLine
                    .append($('<td/>')
                        .append($('<a href="#" onclick="AspectInterpretations.OpenEditForm(' + item.AspectInterpretationId + ');return false" title="Edit Aspect Interpretation"/>').append('<span class="fa fa-edit"/>').append(' Edit'))
                        .append($('<br/>'))
                        .append($('<a href="#" onclick="AspectInterpretations.OpenDeleteForm(' + item.AspectInterpretationId + ');return false" title="Delete Aspect Interpretation"/>').append('<span class="fa fa-remove error"/>')
                            .append($('<span class="error"/>').html(' Delete')))
                    );

                $('#listingBody').append(listingLine);
            });

            $('#pageNumber').empty();
            $('#totalPages').text(data[1]);
            for (var i = 1; i <= parseInt(data[1]); i++) {
                $('#pageNumber').append($((i === page) ? '<option selected="selected"/>' : '<option/>').val(i.toString()).text(i.toString()));
            }
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
                $('#status').html(errorIcon + ' Error: Cannot retrieve Aspect Interpretation Listing');
            });

        $.ajaxSetup({ cache: true });

        return jqxhr;
    }

    export function OpenCreateForm() {
        $('#createAspectInterpretationModal').modal('show');
    }

    export function OpenEditForm(id: number) {
        $('#editAspectInterpretationModal').modal('show');
        GetEditData(id);
    }

    export function OpenDeleteForm(id: number) {
        $('#deleteAspectInterpretationModal').modal('show');

        var jqxhr : JQueryPromise<any> = $.getJSON('/AspectInterpretations/GetAspectInterpretationForDelete', { id: id }).done(function (data) {
            $('#deleteAspectInterpretationId').val(data[0].AspectInterpretationId);
            $('#deleteFirstObject').html(data[0].FirstObject);
            $('#deleteSecondObject').html(data[0].SecondObject);
            $('#deleteAspect').html(data[0].AspectName);
            $('#deleteInterpretation').html(data[0].Interpretation.replace(/\n/g, '<br>'));
            $('#deleteCitationUrl').html(
                (!IsNullOrUndefined(data[0].CitationUrl) && data[0].CitationUrl.substring(0, 4) === 'http'
                ? '<a href="' + data[0].CitationUrl + '" target="_blank">' + data[0].CitationUrl + '</a>'
                : data[0].CitationUrl));
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Delete Load Failure: ' + err);
                $('#status').append($('<div/>').html(errorIcon + ' Failed to get delete data..'));
            });
    }

    $('#confirmAspectInterpretationDelete').click(function () {
        var jqxhr = $.post('/AspectInterpretations/ConfirmDeleteOfAspectInterpretation', {
            aspectInterpretationId: $('#deleteAspectInterpretationId').val()
        }).done(function (data) {
                $('#status').html(successIcon + ' Aspect Interpretation deleted successfully.');

                $('#deleteAspectInterpretationModal').modal('hide');

                listing = true;
                $('#listingBody').empty();
                $listingLoading.show();
                $.when(GetAspectInterpretationListing(parseInt($('#pageNumber').val()), parseInt($('#resultsPerPage').val()))).done(function () {
                    $listingLoading.show();
                    listing = false;
                });
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Delete Load Failure: ' + err);
                $('#status').append($('<div/>').html(errorIcon + ' Failed to delete Aspect Interpretation.'));
            });
    });

    export function SetUpAspectInterpretationDropdowns() {
        $.getJSON('/AspectInterpretations/GetAspectInterpretationTypes').done(function (data) {
            $.each([$('#createAspectInterpretationTypeId1'), $('#createAspectInterpretationTypeId2'), $('#editAspectInterpretationTypeId1'), $('#editAspectInterpretationTypeId2')], function (i, list) {
                $.each(data, function (j, item) {
                    list.append($('<option value="' + item.AspectInterpretationTypeId + '"' + ((j === 0) ? ' selected="selected"' : '') + '/>').html(item.AspectInterpretationTypeName));
                })
        });
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Aspect Interepretation Type List failure.: ' + err);
                $('#status').append($('<div/>').html(errorIcon + ' Failed to retrieve Aspect Interpretation Types.'));
            });
    }

    export function SetUpAspectDropdown() {
        $.getJSON('/AspectInterpretations/GetAspectsList').done(function (data) {
            $.each([$('#createAspectId'), $('#editAspectId')], function (i, list) {
                $.each(data, function (j, item) {
                    list.append($('<option value="' + item.AspectId + '"' + ((j === 0) ? ' selected="selected"' : '') + '/>').html(item.AspectName));
                });
            });
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Aspect List failure.: ' + err);
                $('#status').append($('<div/>').html(errorIcon + ' Failed to retrieve Aspect List Types.'));
            });
    }

    export function SetUpAngleDropdown() {
        $.getJSON('/AspectInterpretations/GetAnglesList').done(function (data) {
            $.each([$('#createAngle1Id'), $('#createAngle2Id'), $('#editAngle1Id'), $('#editAngle2Id')], function (i, list) {
                $.each(data, function (j, item) {
                    list.append($('<option value="' + item.AngleId + '"' + ((j === 0) ? ' selected="selected"' : '') + '/>').html(item.AngleName));
                })
        });
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Aspect List failure.: ' + err);
                $('#status').append($('<div/>').html(errorIcon + ' Failed to retrieve Angle List Types.'));
            });
    }

    export function SetUpIdsForEntry(aspectType1Id, celObj1Id, useCelObj1Id, angleId1, useAngle1Id, aspectType2Id, celObj2Id, useCelObj2Id, angleId2, useAngle2Id) {
        if (useCelObj1Id) {
            if (useCelObj2Id) {
                if (celObj1Id < celObj2Id) {
                    return [parseInt(aspectType1Id), parseInt(celObj1Id), null, parseInt(aspectType2Id), parseInt(celObj2Id), null];
                } else {
                    return [parseInt(aspectType2Id), parseInt(celObj2Id), null, parseInt(aspectType1Id), parseInt(celObj1Id), null];
                }
            } else if (useAngle2Id) {
                return [parseInt(aspectType1Id), parseInt(celObj1Id), null, parseInt(aspectType2Id), null, parseInt(angleId2)];
            }
        } else if (useAngle1Id) {
            if (useCelObj2Id) {
                return [parseInt(aspectType1Id), null, parseInt(angleId1), parseInt(aspectType2Id), parseInt(celObj2Id), null];
            } else if (useAngle2Id) {
                if (angleId1 < angleId2) {
                    return [parseInt(aspectType1Id), null, parseInt(angleId1), parseInt(aspectType2Id), null, parseInt(angleId2)];
                } else {
                    return [parseInt(aspectType2Id), null, parseInt(angleId2), parseInt(aspectType1Id), null, parseInt(angleId1)];
                }
            }
        }
        return null;
    }

    $('#createSelectFirstCelestialObject').click(function () {
        $('#createAngle1Id').attr('disabled', 'disabled');
        $('#createCelestialObject1').removeAttr('disabled');
    });

    $('#createSelectFirstChartAngle').click(function () {
        $('#createAngle1Id').removeAttr('disabled');
        $('#createCelestialObject1').val('').attr('disabled', 'disabled');
        $('#createCelestialObject1Id').empty();
        $('#createCelestialObject1Selected').empty();
    });

    $('#createSelectSecondCelestialObject').click(function () {
        $('#createAngle2Id').attr('disabled', 'disabled');
        $('#createCelestialObject2').removeAttr('disabled');
    });

    $('#createSelectSecondChartAngle').click(function () {
        $('#createAngle2Id').removeAttr('disabled');
        $('#createCelestialObject2').val('').attr('disabled', 'disabled');
        $('#createCelestialObject2Id').empty();
        $('#createCelestialObject2Selected').empty();
    });

    $('#editSelectFirstCelestialObject').click(function () {
        $('#editAngle1Id').attr('disabled', 'disabled');
        $('#editCelestialObject1').removeAttr('disabled');
    });

    $('#editSelectFirstChartAngle').click(function () {
        $('#editAngle1Id').removeAttr('disabled');
        $('#editCelestialObject1').val('').attr('disabled', 'disabled');
        $('#editCelestialObject1Id').empty();
        $('#editCelestialObject1Selected').empty();
    });

    $('#editSelectSecondCelestialObject').click(function () {
        $('#editAngle2Id').attr('disabled', 'disabled');
        $('#editCelestialObject2').removeAttr('disabled');
    });

    $('#editSelectSecondChartAngle').click(function () {
        $('#editAngle2Id').removeAttr('disabled');
        $('#editCelestialObject2').val('').attr('disabled', 'disabled');
        $('#editCelestialObject2Id').empty();
        $('#editCelestialObject2Selected').empty();
    });

    $('#confirmAspectInterpretationCreate').click(function () {
        var sunId = 1, earthId = 5;

        var reorderedIds = SetUpIdsForEntry(
            $('#createAspectInterpretationTypeId1').val(),
            $('#createCelestialObject1Id').val(),
            ($('input[name=createFirstAspectSelection]:checked').val()) === 'celestialObject1' ? true : false,
            $('#createAngle1Id').val(),
            ($('input[name=createFirstAspectSelection]:checked').val()) === 'angle1' ? true : false,
            $('#createAspectInterpretationTypeId2').val(),
            $('#createCelestialObject2Id').val(),
            ($('input[name=createSecondAspectSelection]:checked').val()) === 'celestialObject2' ? true : false,
            $('#createAngle2Id').val(),
            ($('input[name=createSecondAspectSelection]:checked').val()) === 'angle2' ? true : false);

        if (reorderedIds === null) {
            $('#status').html(errorIcon + ' An impossible error occured, but we have this message anyways.');
            return;
        }

        var aspectType1Id = reorderedIds[0],
            celObj1Id = reorderedIds[1],
            angle1Id = reorderedIds[2],
            aspectType2Id = reorderedIds[3],
            celObj2Id = reorderedIds[4],
            angle2Id = reorderedIds[5];

        if (aspectType1Id === 1 && aspectType2Id === 1 && ((!IsNullOrUndefined(celObj1Id) && !IsNullOrUndefined(celObj2Id) && celObj1Id === celObj2Id) || (!IsNullOrUndefined(angle1Id) && !IsNullOrUndefined(angle2Id) && angle1Id === angle2Id))) {
            $('#createStatus').html(errorIcon + ' Impossible aspect: An object or angle in a natal chart cannot aspect itself.');
            return;
        }

        // Sun and Earth cannot aspect each other in any chart (or at least, we haven't gotten to Mars to do Martian Astrology yet).
        if (celObj1Id === sunId && celObj2Id === earthId) {
            $('#createStatus').html(errorIcon + ' Impossible aspect: Sun and Earth cannot aspect each other in any chart yet.  Try again when we colonize Mars.');
            return;
        }

        var jqxhr = $.post('/AspectInterpretations/CreateNewAspectInterpretation', {
            aspectInterpretationTypeId1: aspectType1Id,
            celestialObjectId1: celObj1Id,
            angleId1: angle1Id,
            aspectId: $('#createAspectId').val(),
            aspectInterpretationTypeId2: aspectType2Id,
            celestialObjectId2: celObj2Id,
            angleId2: angle2Id,
            interpretation: $('#createInterpretation').val(),
            citationUrl: $('#createCitationUrl').val()
        }).done(function (data) {
                if (data === 'Success') {
                    $('#status').html(successIcon + ' New interpretation entered successfully.');
                    ResetCreateForm();
                    $('#createAspectInterpretationModal').modal('hide');

                    listing = true;
                    $('#listingBody').empty();
                    $listingLoading.show();
                    $.when(GetAspectInterpretationListing(parseInt($('#pageNumber').val()), parseInt($('#resultsPerPage').val()))).done(function () {
                        $listingLoading.show();
                        listing = false;
                    });
                } else {
                    $('#createStatus').html(errorIcon + ' Failed to enter new Interpretation.<br />' + data);
                }
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('New Interpretation failure: ' + err);
                $('#createStatus').html(errorIcon + ' Failed to enter new Interpretation.');
            });
    });

    $('#confirmAspectInterpretationEdit').click(function () {
        var sunId = 1, earthId = 5;

        var reorderedIds = SetUpIdsForEntry(
            $('#editAspectInterpretationTypeId1').val(),
            $('#editCelestialObject1Id').val(),
            ($('input[name=editFirstAspectSelection]:checked').val()) === 'celestialObject1' ? true : false,
            $('#editAngle1Id').val(),
            ($('input[name=editFirstAspectSelection]:checked').val()) === 'angle1' ? true : false,
            $('#editAspectInterpretationTypeId2').val(),
            $('#editCelestialObject2Id').val(),
            ($('input[name=editSecondAspectSelection]:checked').val()) === 'celestialObject2' ? true : false,
            $('#editAngle2Id').val(),
            ($('input[name=editSecondAspectSelection]:checked').val()) === 'angle2' ? true : false);

        if (reorderedIds === null) {
            $('#editStatus').html(errorIcon + ' An impossible error occured, but we have this message anyways.');
            return;
        }

        var aspectType1Id = reorderedIds[0],
            celObj1Id = reorderedIds[1],
            angle1Id = reorderedIds[2],
            aspectType2Id = reorderedIds[3],
            celObj2Id = reorderedIds[4],
            angle2Id = reorderedIds[5];

        if (aspectType1Id === 1 && aspectType2Id === 1 && (celObj1Id === celObj2Id || (!IsNullOrUndefined(angle1Id) && !IsNullOrUndefined(angle2Id) && angle1Id === angle2Id))) {
            $('#editStatus').html(errorIcon + ' Impossible aspect: An object or angle in a natal chart cannot aspect itself.');
            return;
        }

        // Sun and Earth cannot aspect each other in any chart (or at least, we haven't gotten to Mars to do Martian Astrology yet).
        if (celObj1Id === sunId && celObj2Id === earthId) {
            $('#editStatus').html(errorIcon + ' Impossible aspect: Sun and Earth cannot aspect each other in any chart yet.  Try again when we colonize Mars.');
            return;
        }

        var jqxhr = $.post('/AspectInterpretations/UpdateAspectInterpretation', {
            aspectInterpretationId: $('#editAspectInterpretationId').val(),
            aspectInterpretationTypeId1: aspectType1Id,
            celestialObjectId1: celObj1Id,
            angleId1: angle1Id,
            aspectId: $('#editAspectId').val(),
            aspectInterpretationTypeId2: aspectType2Id,
            celestialObjectId2: celObj2Id,
            angleId2: angle2Id,
            interpretation: $('#editInterpretation').val(),
            citationUrl: $('#editCitationUrl').val()
        }).done(function (data) {
                if (data === 'Success') {
                    $('#status').html(successIcon + ' Aspect interpretation updated successfully.');

                    $('#editAspectInterpretationModal').modal('hide');

                    listing = true;
                    $('#listingBody').empty();
                    $listingLoading.show();
                    $.when(GetAspectInterpretationListing(parseInt($('#pageNumber').val()), parseInt($('#resultsPerPage').val()))).done(function () {
                        $listingLoading.show();
                        listing = false;
                    });
                } else {
                    $('#editStatus').html(errorIcon + ' Failed to update Interpretation.<br />' + data);
                }
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Interpretation Update Failure: ' + err);
                $('#editStatus').html(errorIcon + ' Failed to update Interpretation.');
            });
    });

    export function ResetCreateForm() {
        $.each([$('#createAspectInterpretationTypeId1'), $('#createAspectInterpretationTypeId2'), $('#createAngle1Id'), $('#createAngle2Id')], function (i, item) {
            item.val([]);
        });
        $.each([$('#createCelestialObject1'),
            $('#createCelestialObject1Id'),
            $('#createCelestialObject1Selected'),
            $('#createCelestialObject2'),
            $('#createCelestialObject2Id'),
            $('#createCelestialObject2Selected'),
            $('#createInterpretation'),
            $('#createCitationUrl')], function (i, item) {
                item.val('');
            });
        $('#createStatus').empty();
    }

    export function GetEditData(id: number) {
        var jqxhr = $.getJSON('/AspectInterpretations/GetAspectInterpretationForEdit', { id: id }).done(function (data) {
            $('#editAspectInterpretationId').val(id.toString());
            $('#editAspectInterpretationTypeId1').val(data[0].AspectInterpretationTypeId1);
            if (!IsNullOrUndefined(data[0].CelestialObjectId1)) {
                $('#editSelectFirstCelestialObject').attr('checked', 'checked');
                $('#editCelestialObject1').val(data[0].CelestialObjectName1);
                $('#editCelestialObject1Id').val(data[0].CelestialObjectId1);
                $('#editCelestialObject1Selected').val(data[0].CelestialObjectName1);
                $('#editAngle1Id').attr('disabled', 'disabled');
            } else if (!IsNullOrUndefined(data[0].AngleId1)) {
                $('#editSelectFirstChartAngle').attr('checked', 'checked');
                $('#editAngle1Id').val(data[0].AngleId1);
                $('#editCelestialObject1').attr('disabled', 'disabled');
                $('#editAngle1Id').removeAttr('disabled');
            }
            $('#editAspectInterpretationTypeId2').val(data[0].AspectInterpretationTypeId2);
            if (!IsNullOrUndefined(data[0].CelestialObjectId2)) {
                $('#editSelectSecondCelestialObject').attr('checked', 'checked');
                $('#editCelestialObject2').val(data[0].CelestialObjectName2);
                $('#editCelestialObject2Id').val(data[0].CelestialObjectId2);
                $('#editCelestialObject2Selected').val(data[0].CelestialObjectName2);
                $('#editAngle2Id').attr('disabled', 'disabled');
            } else if (!IsNullOrUndefined(data[0].AngleId2)) {
                $('#editSelectSecondChartAngle').attr('checked', 'checked');
                $('#editAngle2Id').val(data[0].AngleId2);
                $('#editCelestialObject2').attr('disabled', 'disabled');
                $('#editAngle2Id').removeAttr('disabled');
            }
            $('#editAspectId').val(data[0].AspectId);
            $('#editInterpretation').val(data[0].Interpretation);
            $('#editCitationUrl').val(data[0].CitationUrl);
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log('Edit Load Failure: ' + err);
                $('#editStatus').append($('<div/>').html(errorIcon + ' Failed to get edit data..'));
            });
    }

    $('#resultsPerPage').change(function () {
        GetAspectInterpretationListing(1, parseInt($('#resultsPerPage').val()));
    });

    $('#pageNumber').change(function () {
        GetAspectInterpretationListing(parseInt($('#pageNumber').val()), parseInt($('#resultsPerPage').val()));
    });

    $(document).ready(function () {
        GetAspectInterpretationListing(1, 10);

        SetUpAspectInterpretationDropdowns();
        SetUpAspectDropdown();
        SetUpAngleDropdown()

    $('#createAngle1Id').attr('disabled', 'disabled');
        $('#createAngle2Id').attr('disabled', 'disabled');
    });
}