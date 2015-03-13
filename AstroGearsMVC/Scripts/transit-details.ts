/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/jqueryui/jqueryui.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="common-library.ts" />

module TransitDetails {
    var listing1 = false,
        listing2 = false,
        aspects = false;

    var $firstChartLoading = $('#firstChartLoading').hide();
    var $secondChartLoading = $('#secondChartLoading').hide();
    var $aspectloading = $('#transitAspectsLoading').hide();
    $(document)
        .ajaxStart(function () {
            if (listing1 === true) {
                $('#firstChartBody').empty();
                $firstChartLoading.show();
            }
            if (listing2 === true) {
                $('#secondChartBody').empty();
                $secondChartLoading.show();
            }
            if (aspects === true) {
                $aspectloading.show();
            }
        })
        .ajaxStop(function () {
            $firstChartLoading.hide();
            $secondChartLoading.hide();
            $aspectloading.hide();
            listing1 = false;
            listing2 = false;
            aspects = false;
        });

    $('#secondEnteredChartSelection').change(function () {
        var selection = $('#' + $(this).attr('id') + ' option:selected').html();
        $('#secondEnteredChartSubjectLocationLabel').html(selection);
        $('#secondEnteredChartOriginDateTimeLabel').html(selection);
        $('#secondEnteredChartOptions').html(selection);
        $('#secondEnteredChartHouseAngles').html(selection);
        GetSecondChartListing($(this).val());
        GetHouseListing($(this).val(), 2);
        GetSelectedTransitChartData($(this).val());
    });

    export function GetAvailableChartsForTransit(chartId) {
        $.ajaxSetup({ cache: false });
        $.getJSON("/Transits/GetTransitChartListing", { chartId: chartId }).done(function (data) {
            $.each(data, function (i, item) {
                var subName;
                if (item.ChartTypeName === 'Natal') {
                    subName = item.SubjectName;
                } else {
                    subName = item.SubjectName + ' &lt;' + item.ChartTypeName + '&gt;';
                }

                $('#secondEnteredChartSelection').append($('<option value="' + item.EnteredChartId + '" />').html(subName));
            });
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Listing failure: " + err);
            });

        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function GetSelectedTransitChartData(transitChartId) {
        if (transitChartId === '-') {
            $('#secondEnteredChartSubjectLocation').html('');
            $('#secondEnteredChartOriginDateTime').html('');
            return;
        }
        $.ajaxSetup({ cache: false });
        $.getJSON("/Transits/GetSelectedEnteredChartData", { transitChartId: transitChartId }).done(function (data) {
            $('#secondEnteredChartSubjectLocation').html(data.SubjectLocation);
            $('#secondEnteredChartOriginDateTime').html(data.OriginDateTimeString);
            $('#secondEnteredChartTypeId').val(data.ChartTypeId);
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Listing failure: " + err);
            });

        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function RefreshCharts(id, transitId) {
        listing1 = true;
        listing2 = true;

        GetFirstChartListing(id);
        GetHouseListing(id, 1);
        GetSecondChartListing(transitId);
        GetHouseListing(transitId, 2);
    }

    export function GetFirstChartListing(id) {
        listing1 = true;
        aspects = false;
        //$('#chartbody').empty();

        $.ajaxSetup({ cache: false });
        $.getJSON("/EnteredCharts/GetDetailsChartListing", {
            id: id,
            draconic: $('#includeDraconicForFirst').is(':checked'),
            arabic: $('#includeArabicForFirst').is(':checked'),
            asteroids: $('#includeAsteroidsForFirst').is(':checked'),
            stars: $('#includeStarsForFirst').is(':checked'),
            houseSystemId: $('#charthousesystems').val()
        }).done(function (data) {
                $('#firstChartTableHeading').html($('#firstEnteredChartSubjectName').html());
                $.each(data, function (i, item) {
                    var chartLine;
                    if (item.Draconic === true) {
                        chartLine = $('<tr class="draconic"/>');
                    } else {
                        chartLine = $('<tr/>');
                    }
                    var chartFirstCol;
                    switch (item.CelestialObjectTypeName) {
                        case 'Arabic Part':
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="arabic-part draconic"/>');
                            } else {
                                chartFirstCol = $('<td class="arabic-part"/>');
                            }
                            break;
                        case 'Major Planet/Luminary':
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="planet-luminary draconic"/>');
                            } else {
                                chartFirstCol = $('<td class="planet-luminary"/>');
                            }
                            break;
                        case 'Fixed Star':
                            chartFirstCol = $('<td class="fixed-star"/>');
                            break;
                        case 'Angle/House Cusp':
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="house-cusp draconic"/>');
                            } else {
                                chartFirstCol = $('<td class="house-cusp"/>');
                            }
                            break;
                        default:
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="draconic"/>');
                            } else {
                                chartFirstCol = $('<td/>');
                            }
                            break;
                    }
                    chartLine.append(chartFirstCol.html(item.CelestialObjectName));

                    var chartSecondCol = $('<td/>');
                    var orientationString = (!!item.OrientationAbbreviation) ? ' ' + item.OrientationAbbreviation : '';

                    var coordinateString = item.Degrees
                        + '° <span class="'
                        + item.HtmlTextCssClass + '">'
                        + item.SignAbbreviation
                        + '</span> '
                        + item.Minutes
                        + '\' '
                        + item.Seconds
                        + '"'
                        + orientationString;

                    chartSecondCol.html(coordinateString);

                    chartLine.append(chartSecondCol);

                    if (item.House !== 0) {
                        chartLine.append($('<td/>').html(item.House));
                    } else {
                        chartLine.append($('<td/>').html('&nbsp;'));
                    }

                    if (item.Draconic === true) {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspectsForDraconicChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\', 2);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    } else if (item.CelestialObjectTypeName === 'Arabic Part') {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspectsForArabicChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\', 2);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    } else if (item.CelestialObjectTypeName === 'Angle/House Cusp') {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspectsForAngleChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\', 2);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    } else {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspects(' + item.ChartObjectId + ', 2);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    }
                    $('#firstChartBody').append(chartLine);
                });
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
            });

        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function GetSecondChartListing(transitChartId) {
        if (transitChartId === '-') {
            $('#secondChartTableHeading').empty();
            $('#secondEnteredChartHouseAngles').empty();
            $('#secondChartBody').empty();
            return;
        }

        listing2 = true;
        aspects = false;
        //$('#chartbody').empty();

        $.ajaxSetup({ cache: false });
        $.getJSON("/EnteredCharts/GetDetailsChartListing", {
            id: transitChartId,
            draconic: $('#includeDraconicForSecond').is(':checked'),
            arabic: $('#includeArabicForSecond').is(':checked'),
            asteroids: $('#includeAsteroidsForSecond').is(':checked'),
            stars: $('#includeStarsForSecond').is(':checked'),
            houseSystemId: $('#charthousesystems').val()
        }).done(function (data) {
                $('#secondChartTableHeading').html($('#secondEnteredChartSelection option:selected').html());
                $.each(data, function (i, item) {
                    var chartLine;
                    if (item.Draconic === true) {
                        chartLine = $('<tr class="draconic"/>');
                    } else {
                        chartLine = $('<tr/>');
                    }
                    var chartFirstCol;
                    switch (item.CelestialObjectTypeName) {
                        case 'Arabic Part':
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="arabic-part draconic"/>');
                            } else {
                                chartFirstCol = $('<td class="arabic-part"/>');
                            }
                            break;
                        case 'Major Planet/Luminary':
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="planet-luminary draconic"/>');
                            } else {
                                chartFirstCol = $('<td class="planet-luminary"/>');
                            }
                            break;
                        case 'Fixed Star':
                            chartFirstCol = $('<td class="fixed-star"/>');
                            break;
                        case 'Angle/House Cusp':
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="house-cusp draconic"/>');
                            } else {
                                chartFirstCol = $('<td class="house-cusp"/>');
                            }
                            break;
                        default:
                            if (item.Draconic === true) {
                                chartFirstCol = $('<td class="draconic"/>');
                            } else {
                                chartFirstCol = $('<td/>');
                            }
                            break;
                    }
                    chartLine.append(chartFirstCol.html(item.CelestialObjectName));

                    var chartSecondCol = $('<td/>');
                    var orientationString = (!!item.OrientationAbbreviation) ? ' ' + item.OrientationAbbreviation : '';

                    var coordinateString = item.Degrees
                        + '° <span class="'
                        + item.HtmlTextCssClass + '">'
                        + item.SignAbbreviation
                        + '</span> '
                        + item.Minutes
                        + '\' '
                        + item.Seconds
                        + '"'
                        + orientationString;

                    chartSecondCol.html(coordinateString);

                    chartLine.append(chartSecondCol);

                    if (item.House !== 0) {
                        chartLine.append($('<td/>').html(item.House));
                    } else {
                        chartLine.append($('<td/>').html('&nbsp;'));
                    }

                    if (item.Draconic === true) {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspectsForDraconicChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\', 1);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    } else if (item.CelestialObjectTypeName === 'Arabic Part') {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspectsForArabicChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\', 1);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    } else if (item.CelestialObjectTypeName === 'Angle/House Cusp') {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspectsForAngleChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\', 1);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    } else {
                        chartLine.append($('<td/>').append($('<a href="#" onclick="TransitDetails.GetTransitAspects(' + item.ChartObjectId + ', 1);return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                    }
                    $('#secondChartBody').append(chartLine);
                });
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
            });

        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function GetHouseListing(chartId, chartNumber) {
        if (chartId === '-') {
            if (chartNumber === 2) {
                $('#secondChartHouses').hide();
                $('#secondChartDraconicHouses').hide();
            }
            return;
        }

        if (chartNumber === 2) {
            $('#secondChartHouses').show();
        }

        $.ajaxSetup({ cache: false });

        var houseCusps = { 1: 'firstHouse', 2: 'secondHouse', 3: 'thirdHouse', 4: 'fourthHouse', 5: 'fifthHouse', 6: 'sixthHouse', 7: 'seventhHouse', 8: 'eighthHouse', 9: 'ninthHouse', 10: 'tenthHouse', 11: 'eleventhHouse', 12: 'twelfthHouse' };

        var chartNumberIdentifier = { 1: 'ForFirstChart', 2: 'ForSecondChart' };

        var angles = { 0: 'vertex', 1: 'ascendant', 2: 'midheaven', 3: 'antivertex', 4: 'descendant', 5: 'imumCoeli' };

        $.getJSON("/EnteredCharts/GetDetailsHouseListing", { chartId: chartId, houseSystemId: $('#charthousesystems').val() }).done(function (data) {
            $.each(houseCusps, function (i, angle) {
                $('#' + angle + chartNumberIdentifier[chartNumber]).empty();
            });

            $.each(data, function (i, item) {
                var thisHouse = item.Degrees
                    + '° <span class="'
                    + item.HtmlTextCssClass + '">'
                    + item.SignAbbreviation
                    + '</span> '
                    + item.Minutes
                    + '\' '
                    + item.Seconds
                    + '"';
                $('#' + houseCusps[item.HouseId] + chartNumberIdentifier[chartNumber]).html(thisHouse);
            });
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
            });

        $.getJSON("/EnteredCharts/GetDetailsAngleListing", { chartId: chartId }).done(function (data) {
            $.each(angles, function (i, angle) {
                $('#' + angle + chartNumberIdentifier[chartNumber]).empty();
            });

            $.each(data, function (i, item) {
                var thisAngle = item.Degrees
                    + '° <span class="'
                    + item.HtmlTextCssClass + '">'
                    + item.SignAbbreviation
                    + '</span> '
                    + item.Minutes
                    + '\' '
                    + item.Seconds
                    + '"';
                $('#' + angles[item.AngleId] + chartNumberIdentifier[chartNumber]).html(thisAngle);
            });
        }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
            });

        var includeDraconic = chartNumber === 1 ? 'ForFirst' : 'ForSecond';

        var draconicHouses = chartNumber === 1 ? '#firstChart' : '#secondChart';

        if ($('#includeDraconic' + includeDraconic).is(':checked')) {
            $(draconicHouses + 'DraconicHouses').show();
            $.getJSON("/EnteredCharts/GetDetailsDraconicHouseListing", { chartId: chartId, houseSystemId: $('#charthousesystems').val() }).done(function (data) {
                $.each(houseCusps, function (i, angle) {
                    $('#' + angle + chartNumberIdentifier[chartNumber] + 'Draconic').empty();
                });

                $.each(data, function (i, item) {
                    var thisHouse = item.Degrees
                        + '° <span class="'
                        + item.HtmlTextCssClass + '">'
                        + item.SignAbbreviation
                        + '</span> '
                        + item.Minutes
                        + '\' '
                        + item.Seconds
                        + '"';
                    $('#' + houseCusps[item.HouseId] + chartNumberIdentifier[chartNumber] + 'Draconic').html(thisHouse);
                });
            }).fail(function (jqxhrDraconicHouse, textStatus, error) {
                    var err = textStatus + ", " + error;
                    console.log("Chart Reload failure: " + err);
                });

            $.getJSON("/EnteredCharts/GetDetailsDraconicAngleListing", { chartId: chartId }).done(function (data) {
                $.each(angles, function (i, angle) {
                    $('#' + angle + 'Draconic').empty();
                });

                $.each(data, function (i, item) {
                    var thisAngle = item.Degrees
                        + '° <span class="'
                        + item.HtmlTextCssClass + '">'
                        + item.SignAbbreviation
                        + '</span> '
                        + item.Minutes
                        + '\' '
                        + item.Seconds
                        + '"';
                    $('#' + angles[item.AngleId] + chartNumberIdentifier[chartNumber] + 'Draconic').html(thisAngle);
                });
            }).fail(function (jqxhrDraconicAngle, textStatus, error) {
                    var err = textStatus + ", " + error;
                    console.log("Chart Reload failure: " + err);
                });
        } else {
            $('#draconicHouses').hide();
        }

        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }

    function SetUpIdsForEntry(aspectType1Id, celObj1Id, useCelObj1Id, angleId1, useAngle1Id, aspectType2Id, celObj2Id, useCelObj2Id, angleId2, useAngle2Id) {
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

    export function GetTransitAspects(id, chartToCompare) {
        if ($('#secondEnteredChartSelection').val() === '-') {
            return;
        }

        $('#transitAspectListModal').modal('show');
        listing1 = false;
        listing2 = false;
        aspects = true;
        $.ajaxSetup({ cache: false });

        //$('#aspectloading').show();
        $('#firstChartTransitSubject').html((chartToCompare === 2) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html());
        $('#secondChartTransitSubject').html(((chartToCompare === 1) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html()) + '\'s:');
        $('#firstChartTransitObject').empty();
        $('#transitAspectList').empty();

        var headerJqxhr = $.getJSON("/EnteredCharts/GetSelectedAspectChartObject", { id: id }).done(function (data) {
            var orientationString = (!!data.OrientationAbbreviation) ? ' ' + data.OrientationAbbreviation : '';

            $('#firstChartTransitObject').html(data.CelestialObjectName
                + ' ('
                + data.Degrees
                + '° <span class="'
                + data.HtmlTextCssClass + '">'
                + data.SignAbbreviation
                + '</span> '
                + data.Minutes
                + '\' '
                + data.Seconds
                + '"'
                + orientationString
                + ')');
        }).fail(function () {
                console.log("Header Load Fail.");
                return;
            });

        var jqxhr = $.getJSON("/Transits/GetTransitAspectChartObjects",
            {
                id: id,
                firstChartId: (chartToCompare === 2) ? $('#firstEnteredChartId').val() : $('#secondEnteredChartSelection').val(),
                secondChartId: (chartToCompare === 2) ? $('#secondEnteredChartSelection').val() : $('#firstEnteredChartId').val(),
                draconic: $('#includeDraconicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                arabic: $('#includeArabicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                asteroids: $('#includeAsteroidsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                stars: $('#includeStarsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                houseSystemId: $('#charthousesystems').val()
            }).done(function (data) {
                $.each(data, function (i, item) {
                    if (!item.aspectList || !item.aspectList.length) {
                        return true;
                    }

                    var aspectHead = $('<li/>').append($('<span class="' + item.HtmlTextCssClass + '">').text(item.AspectName));

                    var aspectList = $('<ul/>');

                    $.each(item.aspectList, function (j, subitem) {
                        var aspectLine = SetUpListItemElementForAspect(subitem);

                        var newIdName = item.AspectName + j;
                        aspectLine.attr('id', newIdName);

                        var orientationString = (!!subitem.OrientationAbbreviation) ? ' ' + subitem.OrientationAbbreviation : '';
                        var houseString = (subitem.House != 0) ? ' | House ' + subitem.House : '';

                        var interpretationIds = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? SetUpIdsForEntry(
                            (chartToCompare === 2) ? parseInt($('#firstEnteredChartTypeId').val()) : parseInt($('#secondEnteredChartTypeId').val()),
                            subitem.BaseObjectCelestialObjectId, (subitem.BaseObjectCelestialObjectId !== 0) ? true : false,
                            subitem.BaseObjectAngleId, (!IsNullOrUndefined(subitem.BaseObjectAngleId)) ? true : false,
                            (chartToCompare === 2) ? parseInt($('#secondEnteredChartTypeId').val()) : parseInt($('#firstEnteredChartTypeId').val()),
                            subitem.CelestialObjectId, (subitem.CelestialObjectId !== 0) ? true : false,
                            subitem.AngleId, (!IsNullOrUndefined(subitem.AngleId)) ? true : false
                            )
                            : null;

                        var interpretationLink = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? ' <a href="#" onclick="TransitDetails.GetInterpretation(\'#' + newIdName + '\', ' + 
                            interpretationIds[0] + ', ' +
                            interpretationIds[1] + ', ' +
                            interpretationIds[2] + ', ' +
                            item.AspectId + ', ' +
                            interpretationIds[3] + ', ' +
                            interpretationIds[4] + ', ' +
                            interpretationIds[5] + ');return false;"><span class="fa fa-search"></span></a>' : '';

                        aspectLine.html(subitem.CelestialObjectName
                            + ' ('
                            + subitem.Degrees
                            + '° <span class="'
                            + subitem.HtmlTextCssClass + '">'
                            + subitem.SignAbbreviation
                            + '</span> '
                            + subitem.Minutes
                            + '\' '
                            + subitem.Seconds
                            + '"'
                            + orientationString
                            + houseString
                            + ')'
                            + interpretationLink);
                        aspectList.append(aspectLine);
                    });
                    aspectHead.append(aspectList);

                    $('#transitAspectList').append(aspectHead);
                });
            }).fail(function () {
                console.log("Aspect List Load failure..");
            });

        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function GetInterpretation(tag, chartTypeId1, celestialObjectId1, angleId1, aspectId, chartTypeId2, celestialObjectId2, angleId2) {
        if ($(tag + ' > ul').length > 0) {
            $(tag + ' > ul').remove();
            $(tag + ' > a > .fa').removeClass('action-successful');
            return;
        }
        $(tag + ' > a > .fa').addClass('action-successful');

        var interpretationList = $('<ul class="detail-interpretation"/>');

        var jqxhr = $.getJSON('/AspectInterpretations/GetTransitChartDetailAspectInterpretationRequest', {
            chartTypeId1: chartTypeId1,
            celestialObjectId1: celestialObjectId1,
            angleId1: angleId1,
            aspectId: aspectId,
            chartTypeId2: chartTypeId2,
            celestialObjectId2: celestialObjectId2,
            angleId2: angleId2
        }).done(function (data) {
                if (data.length > 0) {
                    $.each(data, function (i, item) {
                        interpretationList.append($('<li/>').html(item.Interpretation.replace(/\n/g, '<br>')
                            + (!IsNullOrUndefined(item.CitationUrl)
                            ? ' (' + (item.CitationUrl.substring(0, 4) === 'http'
                            ? '<a href="' + item.CitationUrl + '" target="_blank">' + item.CitationUrl + '</a>'
                            : item.CitationUrl) + ')'
                            : '')));
                    });
                } else {
                    interpretationList.append($('<li/>').html('(no interpretation available)'));
                }
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Interpretation Load failure: " + err);
            });

        $(tag).append(interpretationList)
    }

    export function GetTransitAspectsForAngleChart(angleName, angleCoordinates, chartToCompare) {
        if ($('#secondEnteredChartSelection').val() === '-') {
            return;
        }

        $('#transitAspectListModal').modal('show');
        listing1 = false;
        listing2 = false;
        aspects = true;
        $.ajaxSetup({ cache: false });

        //$('#aspectloading').show();
        $('#firstChartTransitSubject').html((chartToCompare === 2) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html());
        $('#secondChartTransitSubject').html(((chartToCompare === 1) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html()) + '\'s:');
        $('#firstChartTransitObject').empty();
        $('#transitAspectList').empty();

        $('#firstChartTransitObject').html(angleName
            + ' ('
            + angleCoordinates
            + ')');

        var jqxhr = $.getJSON("/Transits/GetTransitAspectChartObjectsForAngle",
            {
                firstChartId: (chartToCompare === 2) ? $('#firstEnteredChartId').val() : $('#secondEnteredChartSelection').val(),
                secondChartId: (chartToCompare === 2) ? $('#secondEnteredChartSelection').val() : $('#firstEnteredChartId').val(),
                draconic: $('#includeDraconicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                arabic: $('#includeArabicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                asteroids: $('#includeAsteroidsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                stars: $('#includeStarsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                angleName: angleName,
                angleCoordinates: angleCoordinates,
                houseSystemId: $('#charthousesystems').val()
            }).done(function (data) {
                $.each(data, function (i, item) {
                    if (!item.aspectList || !item.aspectList.length) {
                        return true;
                    }

                    var aspectHead = $('<li/>').append($('<span class="' + item.HtmlTextCssClass + '">').text(item.AspectName));

                    var aspectList = $('<ul/>');
                    $.each(item.aspectList, function (j, subitem) {
                        var aspectLine = SetUpListItemElementForAspect(subitem);

                        var newIdName = item.AspectName + j;
                        aspectLine.attr('id', newIdName);

                        var orientationString = (!!subitem.OrientationAbbreviation) ? ' ' + subitem.OrientationAbbreviation : '';
                        var houseString = (subitem.House != 0) ? ' | House ' + subitem.House : '';

                        var interpretationIds = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? SetUpIdsForEntry(
                            parseInt($('#firstEnteredChartTypeId').val()),
                            subitem.BaseObjectCelestialObjectId, (subitem.BaseObjectCelestialObjectId !== 0) ? true : false,
                            subitem.BaseObjectAngleId, (!IsNullOrUndefined(subitem.BaseObjectAngleId)) ? true : false,
                            parseInt($('#secondEnteredChartTypeId').val()),
                            subitem.CelestialObjectId, (subitem.CelestialObjectId !== 0) ? true : false,
                            subitem.AngleId, (!IsNullOrUndefined(subitem.AngleId)) ? true : false
                            )
                            : null;

                        var interpretationLink = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? ' <a href="#" onclick="TransitDetails.GetInterpretation(\'#' + newIdName + '\', ' +
                            interpretationIds[0] + ', ' +
                            interpretationIds[1] + ', ' +
                            interpretationIds[2] + ', ' +
                            item.AspectId + ', ' +
                            interpretationIds[3] + ', ' +
                            interpretationIds[4] + ', ' +
                            interpretationIds[5] + ');return false;"><span class="fa fa-search"></span></a>' : '';

                        aspectLine.html(subitem.CelestialObjectName
                            + ' ('
                            + subitem.Degrees
                            + '° <span class="'
                            + subitem.HtmlTextCssClass + '">'
                            + subitem.SignAbbreviation
                            + '</span> '
                            + subitem.Minutes
                            + '\' '
                            + subitem.Seconds
                            + '"'
                            + orientationString
                            + houseString
                            + ')'
                            + interpretationLink);
                        aspectList.append(aspectLine);
                    });
                    aspectHead.append(aspectList);

                    $('#transitAspectList').append(aspectHead);
                });
            }).fail(function (JqXHR) {
                console.log("Aspect List Load failure..");
            });

        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function GetTransitAspectsForArabicChart(arabicPartName, arabicPartCoordinates, chartToCompare) {
        if ($('#secondEnteredChartSelection').val() === '-') {
            return;
        }

        $('#transitAspectListModal').modal('show');
        listing1 = false;
        listing2 = false;
        aspects = true;
        $.ajaxSetup({ cache: false });

        //$('#aspectloading').show();
        $('#firstChartTransitSubject').html((chartToCompare === 2) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html());
        $('#secondChartTransitSubject').html(((chartToCompare === 1) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html()) + '\'s:');
        $('#firstChartTransitObject').empty();
        $('#transitAspectList').empty();

        $('#firstChartTransitObject').html(arabicPartName
            + ' ('
            + arabicPartCoordinates
            + ')');

        var jqxhr = $.getJSON("/Transits/GetTransitAspectChartObjectsForArabicPart",
            {
                firstChartId: (chartToCompare === 2) ? $('#firstEnteredChartId').val() : $('#secondEnteredChartSelection').val(),
                secondChartId: (chartToCompare === 2) ? $('#secondEnteredChartSelection').val() : $('#firstEnteredChartId').val(),
                arabicPartName: arabicPartName,
                arabicPartCoordinates: arabicPartCoordinates,
                draconic: $('#includeDraconicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                arabic: $('#includeArabicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                asteroids: $('#includeAsteroidsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                stars: $('#includeStarsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                houseSystemId: $('#charthousesystems').val()
            }).done(function (data) {
                $.each(data, function (i, item) {
                    if (!item.aspectList || !item.aspectList.length) {
                        return true;
                    }

                    var aspectHead = $('<li/>').append($('<span class="' + item.HtmlTextCssClass + '">').text(item.AspectName));

                    var aspectList = $('<ul/>');
                    $.each(item.aspectList, function (j, subitem) {
                        var aspectLine = SetUpListItemElementForAspect(subitem);

                        var orientationString = (!!subitem.OrientationAbbreviation) ? ' ' + subitem.OrientationAbbreviation : '';
                        var houseString = (subitem.House != 0) ? ' | House ' + subitem.House : '';

                        aspectLine.html(subitem.CelestialObjectName
                            + ' ('
                            + subitem.Degrees
                            + '° <span class="'
                            + subitem.HtmlTextCssClass + '">'
                            + subitem.SignAbbreviation
                            + '</span> '
                            + subitem.Minutes
                            + '\' '
                            + subitem.Seconds
                            + '"'
                            + orientationString
                            + houseString
                            + ')');
                        aspectList.append(aspectLine);
                    });
                    aspectHead.append(aspectList);

                    $('#transitAspectList').append(aspectHead);
                });
            }).fail(function (JqXHR) {
                console.log("Aspect List Load failure..");
            });

        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function GetTransitAspectsForDraconicChart(draconicName, draconicCoordinates, chartToCompare) {
        if ($('#secondEnteredChartSelection').val() === '-') {
            return;
        }

        $('#transitAspectListModal').modal('show');
        listing1 = false;
        listing2 = false;
        aspects = true;
        $.ajaxSetup({ cache: false });

        //$('#aspectloading').show();
        $('#firstChartTransitSubject').html((chartToCompare === 2) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html());
        $('#secondChartTransitSubject').html(((chartToCompare === 1) ? $('#firstEnteredChartSubjectName').html() : $('#secondEnteredChartSelection option:selected').html()) + '\'s:');
        $('#firstChartTransitObject').empty();
        $('#transitAspectList').empty();

        $('#firstChartTransitObject').html(draconicName
            + ' ('
            + draconicCoordinates
            + ')');

        var jqxhr = $.getJSON("/Transits/GetTransitAspectChartObjectsForDraconicObject",
            {
                firstChartId: (chartToCompare === 2) ? $('#firstEnteredChartId').val() : $('#secondEnteredChartSelection').val(),
                secondChartId: (chartToCompare === 2) ? $('#secondEnteredChartSelection').val() : $('#firstEnteredChartId').val(),
                draconicName: draconicName,
                draconicCoordinates: draconicCoordinates,
                draconic: $('#includeDraconicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                arabic: $('#includeArabicFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                asteroids: $('#includeAsteroidsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                stars: $('#includeStarsFor' + ((chartToCompare === 1) ? 'First' : 'Second')).is(':checked'),
                houseSystemId: $('#charthousesystems').val()
            }).done(function (data) {
                $.each(data, function (i, item) {
                    if (!item.aspectList || !item.aspectList.length) {
                        return true;
                    }

                    var aspectHead = $('<li/>').append($('<span class="' + item.HtmlTextCssClass + '">').text(item.AspectName));

                    var aspectList = $('<ul/>');
                    $.each(item.aspectList, function (j, subitem) {
                        var aspectLine = SetUpListItemElementForAspect(subitem);

                        var orientationString = (!!subitem.OrientationAbbreviation) ? ' ' + subitem.OrientationAbbreviation : '';
                        var houseString = (subitem.House != 0) ? ' | House ' + subitem.House : '';

                        aspectLine.html(subitem.CelestialObjectName
                            + ' ('
                            + subitem.Degrees
                            + '° <span class="'
                            + subitem.HtmlTextCssClass + '">'
                            + subitem.SignAbbreviation
                            + '</span> '
                            + subitem.Minutes
                            + '\' '
                            + subitem.Seconds
                            + '"'
                            + orientationString
                            + houseString
                            + ')');
                        aspectList.append(aspectLine);
                    });
                    aspectHead.append(aspectList);

                    $('#transitAspectList').append(aspectHead);
                });
            }).fail(function (JqXHR) {
                console.log("Aspect List Load failure..");
            });

        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }

    export function SetUpListItemElementForAspect(aspectItem) {
        switch (aspectItem.CelestialObjectTypeName) {
            case 'Arabic Part':
                if (aspectItem.Draconic === true) {
                    return $('<li class="arabic-part draconic"/>');
                } else {
                    return $('<li class="arabic-part"/>');
                }
                break;
            case 'Major Planet/Luminary':
                if (aspectItem.Draconic === true) {
                    return $('<li class="planet-luminary draconic"/>');
                } else {
                    return $('<li class="planet-luminary"/>');
                }
                break;
            case 'Fixed Star':
                return $('<li class="fixed-star"/>');
                break;
            case 'Angle/House Cusp':
                if (aspectItem.Draconic === true) {
                    return $('<li class="house-cusp draconic"/>');
                } else {
                    return $('<li class="house-cusp"/>');
                }
                break;
            default:
                if (aspectItem.Draconic === true) {
                    return $('<li class="draconic"/>');
                } else {
                    return $('<li/>');
                }
                break;
        }
    }

    $(document).ready(function () {
        //GetListing($('#enteredchartid').val()); GetHouseListing($('#enteredchartid').val());
        GetFirstChartListing($('#firstEnteredChartId').val());
        GetHouseListing($('#firstEnteredChartId').val(), 1);
        GetAvailableChartsForTransit($('#firstEnteredChartId').val());
    });
}