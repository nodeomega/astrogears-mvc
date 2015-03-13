$('.celestial-object').autocomplete({
    source: function (request, response) {
        $.getJSON('GetFullAutoComplete', { enteredTerm: request.term }).done(function (data) {
            response($.map(data, function (item) {
                return { id: item.CelestialObjectId, label: item.CelestialObjectName, value: item.CelestialObjectName };
            }));
        }).fail(function (ex) {
            console.log("AutoComplete Failed.");
        })
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

function SetUpAspectInterpretationDropdowns()
{
    $.getJSON('GetAspectInterpretationTypes').done(function (data) {
        $.each([$('#aspectInterpretationTypeId1'), $('#aspectInterpretationTypeId2')], function (i, list) {
            $.each(data, function (j, item) {
                list.append($('<option value="' + item.AspectInterpretationTypeId + '"' + ((j === 0) ? ' selected="selected"' : '') + '/>').html(item.AspectInterpretationTypeName));
            })
        });
    }).fail(function (jqxhr, textStatus, error) {
        var err = textStatus + ", " + error;
        console.log('Apect Interepretation Type List failure.: ' + err);
        $('#status').append($('<div/>').html(errorIcon + ' Failed to retrieve Aspect Interpretation Types.'));
    });
}

function SetUpAspectDropdown()
{
    $.getJSON('GetAspectsList').done(function (data) {
        $.each(data, function (i, item) {
            $('#aspectId').append($('<option value="' + item.AspectId + '"' + ((i === 0) ? ' selected="selected"' : '') + '/>').html(item.AspectName));
        })
    }).fail(function (jqxhr, textStatus, error) {
        var err = textStatus + ", " + error;
        console.log('Apect List failure.: ' + err);
        $('#status').append($('<div/>').html(errorIcon + ' Failed to retrieve Aspect List Types.'));
    });
}

function SetUpAngleDropdown() {
    $.getJSON('GetAnglesList').done(function (data) {
        $.each([$('#angle1Id'), $('#angle2Id')], function (i, list) {
            $.each(data, function (j, item) {
                list.append($('<option value="' + item.AngleId + '"' + ((j === 0) ? ' selected="selected"' : '') + '/>').html(item.AngleName));
            })
        });
    }).fail(function (jqxhr, textStatus, error) {
        var err = textStatus + ", " + error;
        console.log('Apect List failure.: ' + err);
        $('#status').append($('<div/>').html(errorIcon + ' Failed to retrieve Angle List Types.'));
    });
}

function SetUpIdsForEntry(aspectType1Id, celObj1Id, useCelObj1Id, angleId1, useAngle1Id, aspectType2Id, celObj2Id, useCelObj2Id, angleId2, useAngle2Id) {
    if (useCelObj1Id)
    {
        if (useCelObj2Id)
        {
            if (celObj1Id > celObj2Id) {
                return [parseInt(aspectType2Id), parseInt(celObj2Id), null, parseInt(aspectType1Id), parseInt(celObj1Id), null];
            } else {
                return [parseInt(aspectType1Id), parseInt(celObj1Id), null, parseInt(aspectType2Id), parseInt(celObj2Id), null];
            }
        } else if (useAngle2Id) {
            return [parseInt(aspectType1Id), parseInt(celObj1Id), null, parseInt(aspectType2Id), null, parseInt(angleId2)];
        }
    } else if (useAngle1Id) {
        if (useCelObj2Id) {
            return [parseInt(aspectType1Id), null, parseInt(angleId1), parseInt(aspectType2Id), parseInt(celObj2Id), null];
        } else if (useAngle2Id) {
            return [parseInt(aspectType1Id), null, parseInt(angleId1), parseInt(aspectType2Id), null, parseInt(angleId2)];
        }
    }
    return null;
}

$('#addNewAspectInterpretation').click(function () {
    var sunId = 1, earthId = 5;

    var reorderedIds = SetUpIdsForEntry(
        $('#aspectInterpretationTypeId1').val(),
        $('#celestialObject1Id').val(),
        ($('input[name=firstAspectSelection]:checked').val()) === 'celestialObject1' ? true : false,
        $('#angle1Id').val(),
        ($('input[name=firstAspectSelection]:checked').val()) === 'angle1' ? true : false,
        $('#aspectInterpretationTypeId2').val(),
        $('#celestialObject2Id').val(),
        ($('input[name=secondAspectSelection]:checked').val()) === 'celestialObject2' ? true : false,
        $('#angle2Id').val(),
        ($('input[name=secondAspectSelection]:checked').val()) === 'angle2' ? true : false);

    if (reorderedIds === null)
    {
        $('#status').html(errorIcon + ' An impossible error occured, but we have this message anyways.');
        return;
    }

    var aspectType1Id = reorderedIds[0],
        celObj1Id = reorderedIds[1],
        angle1Id = reorderedIds[2],
        aspectType2Id = reorderedIds[3],
        celObj2Id = reorderedIds[4],
        angle2Id = reorderedIds[5];

    if (aspectType1Id === 1 && aspectType2Id === 1 && ((!IsNullOrUndefined(celObj1Id) && !IsNullOrUndefined(celObj2Id) && celObj1Id === celObj2Id) || (!IsNullOrUndefined(angle1Id) && !IsNullOrUndefined(angle2Id) && angle1Id === angle2Id)))
    {
        $('#status').html(errorIcon + ' Impossible aspect: An object or angle in a natal chart cannot aspect itself.');
        return;
    }

    // Sun and Earth cannot aspect each other in any chart (or at least, we haven't gotten to Mars to do Martian Astrology yet).
    if (celObj1Id === sunId && celObj2Id === earthId) {
        $('#status').html(errorIcon + ' Impossible aspect: Sun and Earth cannot aspect each other in any chart yet.  Try again when we colonize Mars.');
        return;
    }

    var jqxhr = $.post('CreateNewAspectInterpretation', {
        aspectInterpretationTypeId1: aspectType1Id,
        celestialObjectId1: celObj1Id,
        angleId1: angle1Id,
        aspectId: $('#aspectId').val(),
        aspectInterpretationTypeId2: aspectType2Id,
        celestialObjectId2: celObj2Id,
        angleId2: angle2Id,
        interpretation: $('#Interpretation').val(),
        citationUrl: $('#CitationUrl').val()
    }).done(function (data) {
        if (data === 'Success') {
            $('#status').html(successIcon + ' New interpretation entered successfully.');
            ResetForm();
        } else {
            $('#status').html(errorIcon + ' Failed to enter new Interpretation.<br />' + data);
        }
    }).fail(function (jqxhr, textStatus, error) {
        var err = textStatus + ", " + error;
        console.log('New Interpretation failure: ' + err);
        $('#status').html(errorIcon + ' Failed to enter new Interpretation.');
    });
});

function ResetForm() {
    $.each([$('#aspectInterpretationTypeId1'), $('#aspectInterpretationTypeId2'), $('#angle1Id'), $('#angle2Id')], function(i, item) {
        item.val([]);
    });
    $.each([$('#celestialObject1'),
        $('#celestialObject1Id'),
        $('#celestialObject1Selected'),
        $('#celestialObject2'),
        $('#celestialObject2Id'),
        $('#celestialObject2Selected'),
        $('#Interpretation'),
        $('#CitationUrl')], function (i, item) {
            item.val('');
        });
}

$('#selectFirstCelestialObject').click(function () {
    $('#angle1Id').attr('disabled', 'disabled');
    $('#celestialObject1').removeAttr('disabled');
});

$('#selectFirstChartAngle').click(function () {
    $('#angle1Id').removeAttr('disabled');
    $('#celestialObject1').val('').attr('disabled', 'disabled');
    $('#celestialObject1Id').empty();
    $('#celestialObject1Selected').empty();
});

$('#selectSecondCelestialObject').click(function () {
    $('#angle2Id').attr('disabled', 'disabled');
    $('#celestialObject2').removeAttr('disabled');
});

$('#selectSecondChartAngle').click(function () {
    $('#angle2Id').removeAttr('disabled');
    $('#celestialObject2').val('').attr('disabled', 'disabled');
    $('#celestialObject2Id').empty();
    $('#celestialObject2Selected').empty();
});

$(document).ready(function () {
    SetUpAspectInterpretationDropdowns();
    SetUpAspectDropdown();
    SetUpAngleDropdown()

    $('#angle1Id').attr('disabled', 'disabled');
    $('#angle2Id').attr('disabled', 'disabled');
});