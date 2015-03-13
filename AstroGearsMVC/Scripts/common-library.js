var errorIcon = '<span class="fa fa-exclamation-triangle error"></span>';
var successIcon = '<span class="fa fa-check-circle action-successful"></span>';
function IsInRange(value, min, max) {
    if ((IsNullOrUndefined(min) || IsNullOrUndefined(max)) || (min > max) || isNaN(value)) {
        return false;
    }
    var intVal = parseInt(value);
    return ((intVal >= min) === (intVal <= max));
}
function IsNullOrUndefined(val) {
    return (typeof val === 'undefined' || val === null);
}
//# sourceMappingURL=common-library.js.map