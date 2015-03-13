/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/jqueryui/jqueryui.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="common-library.ts" />
var EnteredChartDetails;
(function (EnteredChartDetails) {
    var listing = false;
    var aspects = false;
    var asteroidFixedStarAutocomplete;
    $('.degree').focusout(function () {
        if (!IsInRange($(this).val(), 0, 29) && $(this).val()) {
            $(this).css('background-color', '#f99').css('border-color', '#cd0000');
        }
        else {
            $(this).css('background-color', '#fff').css('border-color', '#0000cd');
        }
    });
    $('.minutes').focusout(function () {
        if (!IsInRange($(this).val(), 0, 59) && $(this).val()) {
            $(this).css('background-color', '#f99').css('border-color', '#cd0000');
        }
        else {
            $(this).css('background-color', '#fff').css('border-color', '#0000cd');
        }
    });
    $('.seconds').focusout(function () {
        if (!IsInRange($(this).val(), 0, 59) && $(this).val()) {
            $(this).css('background-color', '#f99').css('border-color', '#cd0000');
        }
        else {
            $(this).css('background-color', '#fff').css('border-color', '#0000cd');
        }
    });
    $('.asteroid-fixed-star-name').autocomplete({
        source: function (request, response) {
            $.getJSON("/EnteredCharts/GetAutoCompleteForAsteroidFixedStar", { chartId: $('#enteredchartid').val(), enteredName: request.term }).done(function (data) {
                response($.map(data, function (item) {
                    return { id: item.CelestialObjectId, label: item.CelestialObjectName, value: item.CelestialObjectName, exists: item.ObjectExists };
                }));
            }).fail(function (ex) {
                console.log("AutoComplete Failed.");
            });
        },
        minLength: 2,
        select: function (event, ui) {
            //console.log(ui.item ?
            //"Selected: " + ui.item.value + " aka " + ui.item.id + " and exists " + ui.item.exists :
            //"Nothing selected, input was " + this.value);
            var thisId = '#' + $(this).attr('id').replace('ObjectName', '') + 'CelestialObjectId';
            var thisSelected = '#' + $(this).attr('id').replace('ObjectName', '') + 'SelectedObjectName';
            if (ui.item) {
                $(thisId).val(ui.item.id);
                $(thisSelected).val(ui.item.value);
                if (ui.item.exists) {
                    $(this).css('background-color', '#fdfd36');
                }
                else {
                    $(this).css('background-color', '#fff');
                }
            }
            else {
                $(thisId).val('');
                $(thisSelected).val($(this).val());
            }
        }
    });
    $('.asteroid-fixed-star-name').blur(function () {
        var thisSelected = '#' + $(this).attr('id').replace('ObjectName', '') + 'SelectedObjectName';
        $(thisSelected).val($(this).val());
    });
    var $chartloading = $('#chartloading').hide();
    var $aspectloading = $('#aspectloading').hide();
    $(document).ajaxStart(function () {
        if (listing === true) {
            $('#chartbody').empty();
            $chartloading.show();
        }
        else if (aspects === true) {
            $aspectloading.show();
        }
    }).ajaxStop(function () {
        $chartloading.hide();
        $aspectloading.hide();
        listing = false;
        aspects = false;
    });
    function OpenEditForm(id) {
        $.ajaxSetup({ cache: false });
        $('#editCoordinatesModal').modal('show');
        var loadJqxhr = $.getJSON("/EnteredCharts/GetSelectedAspectChartObject", { id: id }).done(function (data) {
            var orientationString = (!!data.OrientationAbbreviation) ? ' ' + data.OrientationAbbreviation : '';
            $('#editCoordinatesObjectName').html(data.CelestialObjectName);
            $('#editCoordinatesDegrees').val(("0" + data.Degrees).slice(-2));
            $('#editCoordinatesSigns').val(data.SignId);
            $('#editCoordinatesMinutes').val(("0" + data.Minutes).slice(-2));
            $('#editCoordinatesSeconds').val(("0" + data.Seconds).slice(-2));
            $('#editCoordinatesChartObjectId').val(data.ChartObjectId);
            $('#editCoordinatesOrientations').val(data.OrientationId);
            $('#editCoordinatesEnteredChartId').val(data.EnteredChartID);
        }).fail(function () {
            console.log("Header Load Fail.");
            return;
        });
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.OpenEditForm = OpenEditForm;
    $('#addPlanets').click(function () {
        $.ajaxSetup({ cache: false });
        $('#newPlanetCoordinatesModal').modal('show');
        //var planets = { 1: "Sun", 2: "Moon", 3: "Mercury", 4: "Venus", 5: "Earth", 6: "Mars", 7: "Jupiter", 8: "Saturn", 9: "Uranus", 10: "Neptune", 11: "Pluto" };
        var jqxhr = $.getJSON("/EnteredCharts/GetIfPlanetsExist", { chartId: $('#enteredchartid').val() }).done(function (data) {
            $.each(data, function (i, planet) {
                if (($('charttype').val() !== 'Heliocentric' && planet.CelestialObjectName === 'Earth') || ($('charttype').val() === 'Heliocentric' && planet.CelestialObjectName === "Sun") || planet.ObjectExists) {
                    $('#new' + planet.CelestialObjectName + 'Coordinates').hide();
                }
                else {
                    $('#new' + planet.CelestialObjectName + 'Coordinates').show();
                    $('#new' + planet.CelestialObjectName + 'CoordinatesDegrees').val('');
                    $('#new' + planet.CelestialObjectName + 'CoordinatesSigns').val('---');
                    $('#new' + planet.CelestialObjectName + 'CoordinatesMinutes').val('');
                    $('#new' + planet.CelestialObjectName + 'CoordinatesSeconds').val('');
                    $('#new' + planet.CelestialObjectName + 'CoordinatesOrientations').val('1');
                }
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        $.ajaxSetup({ cache: true });
    });
    $('#addSecondaries').click(function () {
        $.ajaxSetup({ cache: false });
        $('#newSecondaryCoordinatesModal').modal('show');
        //var secondaries = { 12: "Mean Node", 13: "True Node", 14: "Chiron", 15: "Lilith", 106: "Ceres", 107: "Pallas", 108: "Juno", 109: "Vesta" };
        var jqxhr = $.getJSON("/EnteredCharts/GetIfSecondaryObjectsExist", { chartId: $('#enteredchartid').val() }).done(function (data) {
            $.each(data, function (i, obj) {
                if (obj.ObjectExists) {
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'Coordinates').hide();
                }
                else {
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'Coordinates').show();
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'CoordinatesDegrees').val('');
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'CoordinatesSigns').val('---');
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'CoordinatesMinutes').val('');
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'CoordinatesSeconds').val('');
                    $('#new' + obj.CelestialObjectName.replace(' ', '') + 'CoordinatesOrientations').val('1');
                }
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        $.ajaxSetup({ cache: true });
    });
    $('#addAsteroids').click(function () {
        $.ajaxSetup({ cache: false });
        $('#newAsteroidCoordinatesModal').modal('show');
        for (var i = 0; i < 10; i++) {
            $('#newAsteroid' + i + 'CoordinatesObjectName').val('');
            $('#newAsteroid' + i + 'CoordinatesSelectedObjectName').val('');
            $('#newAsteroid' + i + 'CoordinatesObjectName').css('background-color', '#fff');
            $('#newAsteroid' + i + 'CoordinatesDegrees').val('');
            $('#newAsteroid' + i + 'CoordinatesSigns').val('---');
            $('#newAsteroid' + i + 'CoordinatesMinutes').val('');
            $('#newAsteroid' + i + 'CoordinatesSeconds').val('');
            $('#newAsteroid' + i + 'CoordinatesOrientations').val('1');
        }
        $.ajaxSetup({ cache: true });
    });
    $('#saveNewPlanets').click(function () {
        $.ajaxSetup({ cache: false });
        var planets = { 1: "Sun", 2: "Moon", 3: "Mercury", 4: "Venus", 5: "Earth", 6: "Mars", 7: "Jupiter", 8: "Saturn", 9: "Uranus", 10: "Neptune", 11: "Pluto" };
        $.each(planets, function (i, planet) {
            if ($('#new' + planet + 'Coordinates').is(":visible") && IsInRange($('#new' + planet + 'CoordinatesDegrees').val(), 0, 29) && $('#new' + planet + 'CoordinatesSigns').val() !== '---' && IsInRange($('#new' + planet + 'CoordinatesMinutes').val(), 0, 59) && IsInRange($('#new' + planet + 'CoordinatesSeconds').val(), 0, 59)) {
                var jqxhr = $.getJSON("/EnteredCharts/CreateChartObjectForEnteredChart", {
                    enteredChartId: $('#enteredchartid').val(),
                    degrees: $('#new' + planet + 'CoordinatesDegrees').val(),
                    signId: $('#new' + planet + 'CoordinatesSigns').val(),
                    minutes: $('#new' + planet + 'CoordinatesMinutes').val(),
                    seconds: $('#new' + planet + 'CoordinatesSeconds').val(),
                    orientationId: $('#new' + planet + 'CoordinatesOrientations').val(),
                    celestialObjectId: $('#new' + planet + 'CoordinatesCelestialObjectId').val()
                }).done(function (data) {
                    // do nothing
                }).fail(function (jqxhr, textStatus, error) {
                    var err = textStatus + ", " + error;
                    console.log("New Planet Entry failure: " + err);
                });
            }
        });
        listing = true;
        $('#chartbody').empty();
        $chartloading.show();
        GetListing($('#enteredchartid').val());
        HideAddButtons($('#enteredchartid').val());
        $chartloading.hide();
        listing = false;
        $('#newPlanetCoordinatesModal').modal('hide');
        $.ajaxSetup({ cache: true });
    });
    $('#saveNewSecondaries').click(function () {
        $.ajaxSetup({ cache: false });
        var planets = { 12: "MeanNode", 13: "TrueNode", 14: "Chiron", 15: "Lilith", 106: "Ceres", 107: "Pallas", 108: "Juno", 109: "Vesta" };
        $.each(planets, function (i, planet) {
            if ($('#new' + planet + 'Coordinates').is(":visible") && IsInRange($('#new' + planet + 'CoordinatesDegrees').val(), 0, 29) && $('#new' + planet + 'CoordinatesSigns').val() !== '---' && IsInRange($('#new' + planet + 'CoordinatesMinutes').val(), 0, 59) && IsInRange($('#new' + planet + 'CoordinatesSeconds').val(), 0, 59)) {
                var jqxhr = $.getJSON("/EnteredCharts/CreateChartObjectForEnteredChart", {
                    enteredChartId: $('#enteredchartid').val(),
                    degrees: $('#new' + planet + 'CoordinatesDegrees').val(),
                    signId: $('#new' + planet + 'CoordinatesSigns').val(),
                    minutes: $('#new' + planet + 'CoordinatesMinutes').val(),
                    seconds: $('#new' + planet + 'CoordinatesSeconds').val(),
                    orientationId: $('#new' + planet + 'CoordinatesOrientations').val(),
                    celestialObjectId: $('#new' + planet + 'CoordinatesCelestialObjectId').val()
                }).done(function (data) {
                    // do nothing
                }).fail(function (jqxhr, textStatus, error) {
                    var err = textStatus + ", " + error;
                    console.log("New Secondary Object Entry failure: " + err);
                });
            }
        });
        listing = true;
        $('#chartbody').empty();
        $chartloading.show();
        GetListing($('#enteredchartid').val());
        HideAddButtons($('#enteredchartid').val());
        $chartloading.hide();
        listing = false;
        $('#newSecondaryCoordinatesModal').modal('hide');
        $.ajaxSetup({ cache: true });
    });
    $('#saveNewAsteroids').click(function () {
        $.ajaxSetup({ cache: false });
        $.ajaxSetup({ async: false });
        for (var i = 0; i < 10; i++) {
            if (!IsNullOrUndefined($('#newAsteroid' + i + 'CoordinatesSelectedObjectName').val()) && $('#newAsteroid' + i + 'CoordinatesSelectedObjectName').val() !== '') {
                ProcessNewAsteroidOrFixedStar($('#newAsteroid' + i + 'CoordinatesSelectedObjectName').val(), i);
            }
        }
        listing = true;
        $('#chartbody').empty();
        $chartloading.show();
        GetListing($('#enteredchartid').val());
        $chartloading.hide();
        listing = false;
        $('#newAsteroidCoordinatesModal').modal('hide');
        $.ajaxSetup({ async: true });
        $.ajaxSetup({ cache: true });
    });
    function ProcessNewAsteroidOrFixedStar(celestialObjectName, i) {
        var celestialObjectId = 0;
        var jqxhr = $.getJSON("/CelestialObjects/CelestialObjectExists", { celestialObjectName: celestialObjectName }).done(function (data) {
            if (data) {
                celestialObjectId = GetCelestialObjectId(celestialObjectName);
            }
            else {
                celestialObjectId = SaveNewCelestialObject(celestialObjectName);
            }
            if (IsInRange($('#newAsteroid' + i + 'CoordinatesDegrees').val(), 0, 29) && $('#newAsteroid' + i + 'CoordinatesSigns').val() !== '---' && IsInRange($('#newAsteroid' + i + 'CoordinatesMinutes').val(), 0, 59) && IsInRange($('#newAsteroid' + i + 'CoordinatesSeconds').val(), 0, 59)) {
                SaveNewAsteroidOrFixedStar($('#enteredchartid').val(), celestialObjectName, celestialObjectId, $('#newAsteroid' + i + 'CoordinatesDegrees').val(), $('#newAsteroid' + i + 'CoordinatesMinutes').val(), $('#newAsteroid' + i + 'CoordinatesSeconds').val(), $('#newAsteroid' + i + 'CoordinatesSigns').val(), $('#newAsteroid' + i + 'CoordinatesOrientations').val());
            }
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Failed to check existence of celestial object: " + err);
        });
    }
    EnteredChartDetails.ProcessNewAsteroidOrFixedStar = ProcessNewAsteroidOrFixedStar;
    function GetCelestialObjectId(celestialObjectName) {
        var celestialObjectId;
        $.getJSON("/CelestialObjects/GetCelestialObjectId", { celestialObjectName: celestialObjectName }).done(function (data) {
            celestialObjectId = data;
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus = ", " + error;
            console.log("Failed to get Celestial Object Id: " + err);
        });
        return celestialObjectId;
    }
    EnteredChartDetails.GetCelestialObjectId = GetCelestialObjectId;
    function SaveNewCelestialObject(celestialObjectName) {
        var celestialObjectType = celestialObjectName.trim().indexOf('*') === 0 ? 4 : 2;
        var allowableOrb = celestialObjectName.trim().indexOf('*') === 0 ? 1.0 : 1.5;
        var passId;
        var jqxhr = $.getJSON("/CelestialObjects/CreateNewCelestialObject", { celestialObjectName: celestialObjectName, celestialObjectTypeId: celestialObjectType, allowableOrb: allowableOrb }).done(function (data) {
            passId = data;
        });
        return passId;
    }
    EnteredChartDetails.SaveNewCelestialObject = SaveNewCelestialObject;
    function SaveNewAsteroidOrFixedStar(chartId, celestialObjectName, celestialObjectId, degrees, minutes, seconds, sign, orientation) {
        var jqxhr = $.getJSON("/EnteredCharts/CreateChartObjectForEnteredChart", {
            enteredChartId: chartId,
            degrees: degrees,
            signId: sign,
            minutes: minutes,
            seconds: seconds,
            orientationId: orientation,
            celestialObjectId: celestialObjectId
        }).done(function (data) {
            // do nothing
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("New Asteroid or Fixed Star Entry failure: " + err);
        });
    }
    EnteredChartDetails.SaveNewAsteroidOrFixedStar = SaveNewAsteroidOrFixedStar;
    //function OpenHouseEditForm(chartId, houseSystemId) {
    $('#editHouses').click(function () {
        $.ajaxSetup({ cache: false });
        var chartId = $('#enteredchartid').val(), houseSystemId = $('#charthousesystems').val();
        $('#editHouseModal').modal('show');
        $('#editHouseSystem').val(houseSystemId);
        var houseCusps = { 1: 'FirstHouse', 2: 'SecondHouse', 3: 'ThirdHouse', 4: 'FourthHouse', 5: 'FifthHouse', 6: 'SixthHouse', 7: 'SeventhHouse', 8: 'EighthHouse', 9: 'NinthHouse', 10: 'TenthHouse', 11: 'EleventhHouse', 12: 'TwelfthHouse' };
        var angles = { 0: 'Vertex', 1: 'Ascendant', 2: 'Midheaven' };
        var jqxhr = $.getJSON("/EnteredCharts/GetDetailsHouseListing", { chartId: chartId, houseSystemId: houseSystemId }).done(function (data) {
            $.each(houseCusps, function (i, angle) {
                $('#edit' + angle + 'ChartHouseId').val('');
                $('#edit' + angle + 'CoordinatesDegrees').val('');
                $('#edit' + angle + 'CoordinatesSigns').val('---');
                $('#edit' + angle + 'CoordinatesMinutes').val('');
                $('#edit' + angle + 'CoordinatesSeconds').val('');
            });
            $.each(data, function (i, item) {
                $('#edit' + houseCusps[item.HouseId] + 'ChartHouseId').val(item.ChartHouseId);
                $('#edit' + houseCusps[item.HouseId] + 'CoordinatesDegrees').val(("0" + item.Degrees).slice(-2));
                $('#edit' + houseCusps[item.HouseId] + 'CoordinatesSigns').val(item.SignId);
                $('#edit' + houseCusps[item.HouseId] + 'CoordinatesMinutes').val(("0" + item.Minutes).slice(-2));
                $('#edit' + houseCusps[item.HouseId] + 'CoordinatesSeconds').val(("0" + item.Seconds).slice(-2));
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        var jqxhr = $.getJSON("/EnteredCharts/GetDetailsAngleListing", { chartId: chartId }).done(function (data) {
            $.each(angles, function (i, angle) {
                $('#edit' + angle + 'ChartAngleId').val('');
                $('#edit' + angle + 'CoordinatesDegrees').val('');
                $('#edit' + angle + 'CoordinatesSigns').val('---');
                $('#edit' + angle + 'CoordinatesMinutes').val('');
                $('#edit' + angle + 'CoordinatesSeconds').val('');
            });
            $.each(data, function (i, item) {
                $('#edit' + angles[item.AngleId] + 'ChartAngleId').val(item.ChartAngleId);
                $('#edit' + angles[item.AngleId] + 'CoordinatesDegrees').val(("0" + item.Degrees).slice(-2));
                $('#edit' + angles[item.AngleId] + 'CoordinatesSigns').val(item.SignId);
                $('#edit' + angles[item.AngleId] + 'CoordinatesMinutes').val(("0" + item.Minutes).slice(-2));
                $('#edit' + angles[item.AngleId] + 'CoordinatesSeconds').val(("0" + item.Seconds).slice(-2));
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        $.ajaxSetup({ cache: true });
    });
    $('#saveCoordinateChanges').click(function () {
        $.ajaxSetup({ cache: false });
        var loadJqxhr = $.post("/EnteredCharts/UpdateChartObjectForEnteredChart", {
            chartObjectId: $('#editCoordinatesChartObjectId').val(),
            degrees: $('#editCoordinatesDegrees').val(),
            signId: $('#editCoordinatesSigns').val(),
            minutes: $('#editCoordinatesMinutes').val(),
            seconds: $('#editCoordinatesSeconds').val(),
            orientationId: $('#editCoordinatesOrientations').val()
        }).done(function (data) {
            listing = true;
            $('#chartbody').empty();
            $chartloading.show();
            GetListing($('#editCoordinatesEnteredChartId').val());
            $chartloading.hide();
            listing = false;
        }).fail(function (jqXHR) {
            console.log("Save Fail.");
            return;
        });
        $('#editCoordinatesModal').modal('hide');
        $.ajaxSetup({ cache: true });
    });
    function SaveHouseSystemChanges() {
        $.ajaxSetup({ cache: false });
        var houseCusps = {
            1: 'FirstHouse',
            2: 'SecondHouse',
            3: 'ThirdHouse',
            4: 'FourthHouse',
            5: 'FifthHouse',
            6: 'SixthHouse',
            7: 'SeventhHouse',
            8: 'EighthHouse',
            9: 'NinthHouse',
            10: 'TenthHouse',
            11: 'EleventhHouse',
            12: 'TwelfthHouse'
        };
        var angles = { 0: 'Vertex', 1: 'Ascendant', 2: 'Midheaven' };
        var postEnds = [];
        $.each(houseCusps, function (i, cusp) {
            if ($('#edit' + cusp + 'CoordinatesDegrees').val() === '--' || $('#edit' + cusp + 'CoordinatesSigns').val() === '---' || $('#edit' + cusp + 'CoordinatesMinutes').val() === '--' || $('#edit' + cusp + 'CoordinatesSeconds').val() === '--') {
                return;
            }
            if ($('#edit' + cusp + 'ChartHouseId').val() == '') {
                var loadJqxhr = $.post("/EnteredCharts/CreateHouseCuspForEnteredChart", {
                    enteredChartId: $('#enteredchartid').val(),
                    degrees: $('#edit' + cusp + 'CoordinatesDegrees').val(),
                    signId: $('#edit' + cusp + 'CoordinatesSigns').val(),
                    minutes: $('#edit' + cusp + 'CoordinatesMinutes').val(),
                    seconds: $('#edit' + cusp + 'CoordinatesSeconds').val(),
                    houseSystemId: $('#editHouseSystem').val(),
                    houseId: i
                }).done(function (data) {
                    //listing = true;
                    //$('#chartbody').empty();
                    //$chartloading.show();
                    //GetListing($('#editCoordinatesEnteredChartId').val());
                    //$chartloading.hide();
                    //listing = false;
                }).fail(function (jqXHR) {
                    console.log("Save Fail.");
                    return;
                });
                postEnds.push(loadJqxhr);
            }
            else {
                var loadJqxhr = $.post("/EnteredCharts/UpdateHouseCuspForEnteredChart", {
                    chartHouseId: $('#edit' + cusp + 'ChartHouseId').val(),
                    degrees: $('#edit' + cusp + 'CoordinatesDegrees').val(),
                    signId: $('#edit' + cusp + 'CoordinatesSigns').val(),
                    minutes: $('#edit' + cusp + 'CoordinatesMinutes').val(),
                    seconds: $('#edit' + cusp + 'CoordinatesSeconds').val(),
                    houseSystemId: $('#editHouseSystem').val()
                }).done(function (data) {
                    //listing = true;
                    //$('#chartbody').empty();
                    //$chartloading.show();
                    //GetListing($('#editCoordinatesEnteredChartId').val());
                    //$chartloading.hide();
                    //listing = false;
                }).fail(function (jqXHR) {
                    console.log("Save Fail.");
                    return;
                });
                postEnds.push(loadJqxhr);
            }
        });
        $.each(angles, function (i, angle) {
            if ($('#edit' + angle + 'CoordinatesDegrees').val() === '--' || $('#edit' + angle + 'CoordinatesSigns').val() === '---' || $('#edit' + angle + 'CoordinatesMinutes').val() === '--' || $('#edit' + angle + 'CoordinatesSeconds').val() === '--') {
                return;
            }
            if ($('#edit' + angle + 'ChartAngleId').val() == '') {
                var loadJqxhr = $.post("/EnteredCharts/CreateAngleForEnteredChart", {
                    enteredChartId: $('#enteredchartid').val(),
                    degrees: $('#edit' + angle + 'CoordinatesDegrees').val(),
                    signId: $('#edit' + angle + 'CoordinatesSigns').val(),
                    minutes: $('#edit' + angle + 'CoordinatesMinutes').val(),
                    seconds: $('#edit' + angle + 'CoordinatesSeconds').val(),
                    angleId: i
                }).done(function (data) {
                    //listing = true;
                    //$('#chartbody').empty();
                    //$chartloading.show();
                    //GetListing($('#editCoordinatesEnteredChartId').val());
                    //$chartloading.hide();
                    //listing = false;
                }).fail(function (jqXHR) {
                    console.log("Save Fail.");
                    return;
                });
                postEnds.push(loadJqxhr);
            }
            else {
                var loadJqxhr = $.post("/EnteredCharts/UpdateAngleForEnteredChart", {
                    chartAngleId: $('#edit' + angle + 'ChartAngleId').val(),
                    degrees: $('#edit' + angle + 'CoordinatesDegrees').val(),
                    signId: $('#edit' + angle + 'CoordinatesSigns').val(),
                    minutes: $('#edit' + angle + 'CoordinatesMinutes').val(),
                    seconds: $('#edit' + angle + 'CoordinatesSeconds').val()
                }).done(function (data) {
                    //listing = true;
                    //$('#chartbody').empty();
                    //$chartloading.show();
                    //GetListing($('#editCoordinatesEnteredChartId').val());
                    //$chartloading.hide();
                    //listing = false;
                }).fail(function (jqXHR) {
                    console.log("Save Fail.");
                    return;
                });
                postEnds.push(loadJqxhr);
            }
        });
        $.when(postEnds).done(function () {
            listing = true;
            $('#chartbody').empty();
            $chartloading.show();
            GetListing($('#enteredchartid').val());
            GetHouseListing($('#enteredchartid').val());
            $chartloading.hide();
            listing = false;
        });
        $('#editHouseModal').modal('hide');
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.SaveHouseSystemChanges = SaveHouseSystemChanges;
    function GetHouseListing(chartId) {
        $.ajaxSetup({ cache: false });
        var houseCusps = { 1: 'firstHouse', 2: 'secondHouse', 3: 'thirdHouse', 4: 'fourthHouse', 5: 'fifthHouse', 6: 'sixthHouse', 7: 'seventhHouse', 8: 'eighthHouse', 9: 'ninthHouse', 10: 'tenthHouse', 11: 'eleventhHouse', 12: 'twelfthHouse' };
        var angles = { 0: 'vertex', 1: 'ascendant', 2: 'midheaven', 3: 'antivertex', 4: 'descendant', 5: 'imumCoeli' };
        var jqxhr = $.getJSON("/EnteredCharts/GetDetailsHouseListing", { chartId: chartId, houseSystemId: $('#charthousesystems').val() }).done(function (data) {
            $.each(houseCusps, function (i, angle) {
                $('#' + angle).empty();
            });
            $.each(data, function (i, item) {
                var thisHouse = item.Degrees + '° <span class="' + item.HtmlTextCssClass + '">' + item.SignAbbreviation + '</span> ' + item.Minutes + '\' ' + item.Seconds + '"';
                $('#' + houseCusps[item.HouseId]).html(thisHouse);
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        var jqxhr = $.getJSON("/EnteredCharts/GetDetailsAngleListing", { chartId: chartId }).done(function (data) {
            $.each(angles, function (i, angle) {
                $('#' + angle).empty();
            });
            $.each(data, function (i, item) {
                var thisAngle = item.Degrees + '° <span class="' + item.HtmlTextCssClass + '">' + item.SignAbbreviation + '</span> ' + item.Minutes + '\' ' + item.Seconds + '"';
                $('#' + angles[item.AngleId]).html(thisAngle);
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        if ($('#includedraconic').is(':checked')) {
            $('#draconicHouses').show();
            var jqxhr = $.getJSON("/EnteredCharts/GetDetailsDraconicHouseListing", { chartId: chartId, houseSystemId: $('#charthousesystems').val() }).done(function (data) {
                $.each(houseCusps, function (i, angle) {
                    $('#' + angle + 'Draconic').empty();
                });
                $.each(data, function (i, item) {
                    var thisHouse = item.Degrees + '° <span class="' + item.HtmlTextCssClass + '">' + item.SignAbbreviation + '</span> ' + item.Minutes + '\' ' + item.Seconds + '"';
                    $('#' + houseCusps[item.HouseId] + 'Draconic').html(thisHouse);
                });
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
            });
            var jqxhr = $.getJSON("/EnteredCharts/GetDetailsDraconicAngleListing", { chartId: chartId }).done(function (data) {
                $.each(angles, function (i, angle) {
                    $('#' + angle + 'Draconic').empty();
                });
                $.each(data, function (i, item) {
                    var thisAngle = item.Degrees + '° <span class="' + item.HtmlTextCssClass + '">' + item.SignAbbreviation + '</span> ' + item.Minutes + '\' ' + item.Seconds + '"';
                    $('#' + angles[item.AngleId] + 'Draconic').html(thisAngle);
                });
            }).fail(function (jqxhr, textStatus, error) {
                var err = textStatus + ", " + error;
                console.log("Chart Reload failure: " + err);
            });
        }
        else {
            $('#draconicHouses').hide();
        }
        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.GetHouseListing = GetHouseListing;
    function HideAddButtons(chartId) {
        var jqxhr = $.getJSON("/EnteredCharts/GetExistenceOfPlanetsAndSecondaries", { chartId: chartId }).done(function (data) {
            try {
                data[0].AllPlanetsAndLuminariesExist ? $('#addPlanets').hide() : $('#addPlanets').show();
                data[0].AllSecondaryObjectsExist ? $('#addSecondaries').hide() : $('#addSecondaries').show();
            }
            catch (err) {
                console.log(err);
                $('#addPlanets').show();
                $('#addSecondaries').show();
            }
        });
    }
    EnteredChartDetails.HideAddButtons = HideAddButtons;
    function GetListing(id) {
        listing = true;
        aspects = false;
        //$('#chartbody').empty();
        $.ajaxSetup({ cache: false });
        var jqxhr = $.getJSON("/EnteredCharts/GetDetailsChartListing", {
            id: id,
            draconic: $('#includedraconic').is(':checked'),
            arabic: $('#includearabic').is(':checked'),
            asteroids: $('#includeasteroids').is(':checked'),
            stars: $('#includestars').is(':checked'),
            houseSystemId: $('#charthousesystems').val()
        }).done(function (data) {
            $.each(data, function (i, item) {
                var chartLine;
                if (item.Draconic === true) {
                    chartLine = $('<tr class="draconic"/>');
                }
                else {
                    chartLine = $('<tr/>');
                }
                var chartFirstCol;
                switch (item.CelestialObjectTypeName) {
                    case 'Arabic Part':
                        if (item.Draconic === true) {
                            chartFirstCol = $('<td class="arabic-part draconic"/>');
                        }
                        else {
                            chartFirstCol = $('<td class="arabic-part"/>');
                        }
                        break;
                    case 'Major Planet/Luminary':
                        if (item.Draconic === true) {
                            chartFirstCol = $('<td class="planet-luminary draconic"/>');
                        }
                        else {
                            chartFirstCol = $('<td class="planet-luminary"/>');
                        }
                        break;
                    case 'Fixed Star':
                        chartFirstCol = $('<td class="fixed-star"/>');
                        break;
                    case 'Angle/House Cusp':
                        if (item.Draconic === true) {
                            chartFirstCol = $('<td class="house-cusp draconic"/>');
                        }
                        else {
                            chartFirstCol = $('<td class="house-cusp"/>');
                        }
                        break;
                    default:
                        if (item.Draconic === true) {
                            chartFirstCol = $('<td class="draconic"/>');
                        }
                        else {
                            chartFirstCol = $('<td/>');
                        }
                        break;
                }
                chartLine.append(chartFirstCol.html(item.CelestialObjectName));
                var chartSecondCol = $('<td/>');
                var orientationString = (!!item.OrientationAbbreviation) ? ' ' + item.OrientationAbbreviation : '';
                var coordinateString = item.Degrees + '° <span class="' + item.HtmlTextCssClass + '">' + item.SignAbbreviation + '</span> ' + item.Minutes + '\' ' + item.Seconds + '"' + orientationString;
                chartSecondCol.html(coordinateString);
                chartLine.append(chartSecondCol);
                if (item.House != 0) {
                    chartLine.append($('<td/>').html(item.House));
                }
                else {
                    chartLine.append($('<td/>').html('&nbsp;'));
                }
                if (item.Draconic === true) {
                    chartLine.append($('<td/>').append($('<a href="#" onclick="EnteredChartDetails.GetAspectsForDraconicChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\');return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                }
                else if (item.CelestialObjectTypeName === 'Arabic Part') {
                    chartLine.append($('<td/>').append($('<a href="#" onclick="EnteredChartDetails.GetAspectsForArabicChart(\'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\');return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                }
                else if (item.CelestialObjectTypeName === 'Angle/House Cusp') {
                    chartLine.append($('<td/>').append($('<a href="#" onclick="EnteredChartDetails.GetAspectsForAngleChart(' + item.AngleId + ', \'' + item.CelestialObjectName + '\', \'' + coordinateString.replace(/\'/g, '\\&#39;').replace(/"/g, '&quot;').replace(/\</g, '&lt;').replace(/\>/g, '&gt;') + '\');return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')));
                }
                else {
                    chartLine.append($('<td/>').append($('<a href="#" onclick="EnteredChartDetails.GetAspects(' + item.ChartObjectId + ');return false;" title="View Aspects"/>').append('<span class="fa fa-search"/>')).append($('<a href="#" onclick="EnteredChartDetails.OpenEditForm(' + item.ChartObjectId + ');return false;" title="Edit Coordinates"/>').append('<span class="fa fa-edit"/>')));
                }
                $('#chartbody').append(chartLine);
            });
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Chart Reload failure: " + err);
        });
        //$('#chartloading').hide();
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.GetListing = GetListing;
    function GetAspects(id) {
        listing = false;
        aspects = true;
        $.ajaxSetup({ cache: false });
        //$('#aspectloading').show();
        $('#aspecttarget').empty();
        $('#aspectlist').empty();
        var headerJqxhr = $.getJSON("/EnteredCharts/GetSelectedAspectChartObject", { id: id }).done(function (data) {
            var orientationString = (!!data.OrientationAbbreviation) ? ' ' + data.OrientationAbbreviation : '';
            $('#aspecttarget').html('Aspects to ' + data.CelestialObjectName + ' (' + data.Degrees + '° <span class="' + data.HtmlTextCssClass + '">' + data.SignAbbreviation + '</span> ' + data.Minutes + '\' ' + data.Seconds + '"' + orientationString + ')');
        }).fail(function () {
            console.log("Header Load Fail.");
            return;
        });
        var jqxhr = $.getJSON("/EnteredCharts/GetAspectChartObjects", { id: id, chartId: $('#enteredchartid').val(), draconic: $('#includedraconic').is(':checked'), arabic: $('#includearabic').is(':checked'), asteroids: $('#includeasteroids').is(':checked'), stars: $('#includestars').is(':checked'), houseSystemId: $('#charthousesystems').val() }).done(function (data) {
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
                    var interpretationIds = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? SetUpIdsForEntry(subitem.BaseObjectCelestialObjectId, (subitem.BaseObjectCelestialObjectId !== 0) ? true : false, subitem.BaseObjectAngleId, (!IsNullOrUndefined(subitem.BaseObjectAngleId)) ? true : false, subitem.CelestialObjectId, (subitem.CelestialObjectId !== 0) ? true : false, subitem.AngleId, (!IsNullOrUndefined(subitem.AngleId)) ? true : false) : null;
                    var interpretationLink = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? ' <a href="#" onclick="EnteredChartDetails.GetInterpretation(\'#' + newIdName + '\', ' + interpretationIds[0] + ', ' + interpretationIds[1] + ', ' + item.AspectId + ', ' + interpretationIds[2] + ', ' + interpretationIds[3] + ');return false;"><span class="fa fa-search"></span></a>' : '';
                    aspectLine.html(subitem.CelestialObjectName + ' (' + subitem.Degrees + '° <span class="' + subitem.HtmlTextCssClass + '">' + subitem.SignAbbreviation + '</span> ' + subitem.Minutes + '\' ' + subitem.Seconds + '"' + orientationString + houseString + ')' + interpretationLink);
                    aspectList.append(aspectLine);
                });
                aspectHead.append(aspectList);
                $('#aspectlist').append(aspectHead);
            });
        }).fail(function () {
            console.log("Aspect List Load failure..");
        });
        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.GetAspects = GetAspects;
    function GetInterpretation(tag, celestialObjectId1, angleId1, aspectId, celestialObjectId2, angleId2) {
        if ($(tag + ' > ul').length > 0) {
            $(tag + ' > ul').remove();
            $(tag + ' > a > .fa').removeClass('action-successful');
            return;
        }
        $(tag + ' > a > .fa').addClass('action-successful');
        var interpretationList = $('<ul class="detail-interpretation"/>');
        var jqxhr = $.getJSON('/AspectInterpretations/GetSingleChartDetailAspectInterpretationRequest', {
            celestialObjectId1: celestialObjectId1,
            angleId1: angleId1,
            aspectId: aspectId,
            celestialObjectId2: celestialObjectId2,
            angleId2: angleId2
        }).done(function (data) {
            if (data.length > 0) {
                $.each(data, function (i, item) {
                    interpretationList.append($('<li/>').html(item.Interpretation.replace(/\n/g, '<br>') + (!IsNullOrUndefined(item.CitationUrl) ? ' (' + (item.CitationUrl.substring(0, 4) === 'http' ? '<a href="' + item.CitationUrl + '" target="_blank">' + item.CitationUrl + '</a>' : item.CitationUrl) + ')' : '')));
                });
            }
            else {
                interpretationList.append($('<li/>').html('(no interpretation available)'));
            }
        }).fail(function (jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Interpretation Load failure: " + err);
        });
        $(tag).append(interpretationList);
    }
    EnteredChartDetails.GetInterpretation = GetInterpretation;
    function SetUpIdsForEntry(celObj1Id, useCelObj1Id, angleId1, useAngle1Id, celObj2Id, useCelObj2Id, angleId2, useAngle2Id) {
        if (useCelObj1Id) {
            if (useCelObj2Id) {
                if (celObj1Id > celObj2Id) {
                    return [parseInt(celObj1Id), null, parseInt(celObj2Id), null];
                }
                else {
                    return [parseInt(celObj2Id), null, parseInt(celObj1Id), null];
                }
            }
            else if (useAngle2Id) {
                return [parseInt(celObj1Id), null, null, parseInt(angleId2)];
            }
        }
        else if (useAngle1Id) {
            if (useCelObj2Id) {
                return [null, parseInt(angleId1), parseInt(celObj2Id), null];
            }
            else if (useAngle2Id) {
                if (angleId1 > angleId2) {
                    return [null, parseInt(angleId1), null, parseInt(angleId2)];
                }
                else {
                    return [null, parseInt(angleId2), null, parseInt(angleId1)];
                }
            }
        }
        return null;
    }
    function GetAspectsForArabicChart(arabicPartName, arabicPartCoordinates) {
        listing = false;
        aspects = true;
        $.ajaxSetup({ cache: false });
        //$('#aspectloading').show();
        $('#aspecttarget').empty();
        $('#aspectlist').empty();
        $('#aspecttarget').html('Aspects to ' + arabicPartName + ' (' + arabicPartCoordinates + ')');
        var jqxhr = $.getJSON("/EnteredCharts/GetAspectChartObjectsForArabicPart", { chartId: $('#enteredchartid').val(), arabicPartName: arabicPartName, arabicPartCoordinates: arabicPartCoordinates, draconic: $('#includedraconic').is(':checked'), arabic: $('#includearabic').is(':checked'), asteroids: $('#includeasteroids').is(':checked'), stars: $('#includestars').is(':checked'), houseSystemId: $('#charthousesystems').val() }).done(function (data) {
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
                    aspectLine.html(subitem.CelestialObjectName + ' (' + subitem.Degrees + '° <span class="' + subitem.HtmlTextCssClass + '">' + subitem.SignAbbreviation + '</span> ' + subitem.Minutes + '\' ' + subitem.Seconds + '"' + orientationString + houseString + ')');
                    aspectList.append(aspectLine);
                });
                aspectHead.append(aspectList);
                $('#aspectlist').append(aspectHead);
            });
        }).fail(function (JqXHR) {
            console.log("Aspect List Load failure..");
        });
        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.GetAspectsForArabicChart = GetAspectsForArabicChart;
    function GetAspectsForAngleChart(angleId, angleName, angleCoordinates) {
        listing = false;
        aspects = true;
        $.ajaxSetup({ cache: false });
        //$('#aspectloading').show();
        $('#aspecttarget').empty();
        $('#aspectlist').empty();
        $('#aspecttarget').html('Aspects to ' + angleName + ' (' + angleCoordinates + ')');
        var jqxhr = $.getJSON("/EnteredCharts/GetAspectChartObjectsForAngle", { chartId: $('#enteredchartid').val(), angleName: angleName, angleCoordinates: angleCoordinates, draconic: $('#includedraconic').is(':checked'), arabic: $('#includearabic').is(':checked'), asteroids: $('#includeasteroids').is(':checked'), stars: $('#includestars').is(':checked'), houseSystemId: $('#charthousesystems').val() }).done(function (data) {
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
                    var interpretationIds = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? SetUpIdsForEntry(subitem.BaseObjectCelestialObjectId, (subitem.BaseObjectCelestialObjectId !== 0) ? true : false, subitem.BaseObjectAngleId, (!IsNullOrUndefined(subitem.BaseObjectAngleId)) ? true : false, subitem.CelestialObjectId, (subitem.CelestialObjectId !== 0) ? true : false, subitem.AngleId, (!IsNullOrUndefined(subitem.AngleId)) ? true : false) : null;
                    var interpretationLink = (subitem.BaseObjectValidForInterpretation && subitem.ThisObjectValidForInterpretation) ? ' <a href="#" onclick="EnteredChartDetails.GetInterpretation(\'#' + newIdName + '\', ' + interpretationIds[0] + ', ' + interpretationIds[1] + ', ' + item.AspectId + ', ' + interpretationIds[2] + ', ' + interpretationIds[3] + ');return false;"><span class="fa fa-search"></span></a>' : '';
                    aspectLine.html(subitem.CelestialObjectName + ' (' + subitem.Degrees + '° <span class="' + subitem.HtmlTextCssClass + '">' + subitem.SignAbbreviation + '</span> ' + subitem.Minutes + '\' ' + subitem.Seconds + '"' + orientationString + houseString + ')' + interpretationLink);
                    aspectList.append(aspectLine);
                });
                aspectHead.append(aspectList);
                $('#aspectlist').append(aspectHead);
            });
        }).fail(function (JqXHR) {
            console.log("Aspect List Load failure..");
        });
        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.GetAspectsForAngleChart = GetAspectsForAngleChart;
    function GetAspectsForDraconicChart(draconicName, draconicCoordinates) {
        listing = false;
        aspects = true;
        $.ajaxSetup({ cache: false });
        //$('#aspectloading').show();
        $('#aspecttarget').empty();
        $('#aspectlist').empty();
        $('#aspecttarget').html('Aspects to ' + draconicName + ' (' + draconicCoordinates + ')');
        var jqxhr = $.getJSON("/EnteredCharts/GetAspectChartObjectsForDraconicObject", { chartId: $('#enteredchartid').val(), draconicName: draconicName, draconicCoordinates: draconicCoordinates, draconic: $('#includedraconic').is(':checked'), arabic: $('#includearabic').is(':checked'), asteroids: $('#includeasteroids').is(':checked'), stars: $('#includestars').is(':checked'), houseSystemId: $('#charthousesystems').val() }).done(function (data) {
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
                    aspectLine.html(subitem.CelestialObjectName + ' (' + subitem.Degrees + '° <span class="' + subitem.HtmlTextCssClass + '">' + subitem.SignAbbreviation + '</span> ' + subitem.Minutes + '\' ' + subitem.Seconds + '"' + orientationString + houseString + ')');
                    aspectList.append(aspectLine);
                });
                aspectHead.append(aspectList);
                $('#aspectlist').append(aspectHead);
            });
        }).fail(function (JqXHR) {
            console.log("Aspect List Load failure..");
        });
        //$('#aspectloading').hide();
        $.ajaxSetup({ cache: true });
    }
    EnteredChartDetails.GetAspectsForDraconicChart = GetAspectsForDraconicChart;
    function SetUpListItemElementForAspect(aspectItem) {
        switch (aspectItem.CelestialObjectTypeName) {
            case 'Arabic Part':
                if (aspectItem.Draconic === true) {
                    return $('<li class="arabic-part draconic"/>');
                }
                else {
                    return $('<li class="arabic-part"/>');
                }
                break;
            case 'Major Planet/Luminary':
                if (aspectItem.Draconic === true) {
                    return $('<li class="planet-luminary draconic"/>');
                }
                else {
                    return $('<li class="planet-luminary"/>');
                }
                break;
            case 'Fixed Star':
                return $('<li class="fixed-star"/>');
                break;
            case 'Angle/House Cusp':
                if (aspectItem.Draconic === true) {
                    return $('<li class="house-cusp draconic"/>');
                }
                else {
                    return $('<li class="house-cusp"/>');
                }
                break;
            default:
                if (aspectItem.Draconic === true) {
                    return $('<li class="draconic"/>');
                }
                else {
                    return $('<li/>');
                }
                break;
        }
    }
    EnteredChartDetails.SetUpListItemElementForAspect = SetUpListItemElementForAspect;
    function GetAsteroidAndFixedStartAutocomplete() {
        $.getJSON("/EnteredCharts/GetFullAutoCompleteForAsteroidFixedStar", { chartId: $('#enteredchartid').val() }).done(function (data) {
            asteroidFixedStarAutocomplete = data;
        }).fail(function (ex) {
            console.log("AutoComplete Failed.");
        });
    }
    EnteredChartDetails.GetAsteroidAndFixedStartAutocomplete = GetAsteroidAndFixedStartAutocomplete;
    $(document).ready(function () {
        GetListing($('#enteredchartid').val());
        GetHouseListing($('#enteredchartid').val());
        HideAddButtons($('#enteredchartid').val());
    });
})(EnteredChartDetails || (EnteredChartDetails = {}));
//# sourceMappingURL=entered-chart-details.js.map