///////////////////////////////////////////////////////////////////////////////
// Convert

var ss_Convert = function Convert$() {
};

ss_Convert.toSingle = function Convert$toSingle(value) {
  var typeName = typeof value;
  if(typeName === "boolean") {
    return (value == true) ? 1.0 : 0.0;
  }
  if(typeName === "number") {
    return parseFloat(value);
  }
  if(typeName === "string") {
    if(!isNaN(value))
      return parseFloat(value);
  }
  throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "Single".');
};

ss_Convert.toDouble = function Convert$toDouble(value) {
    var typeName = typeof value;
    if (typeName === "boolean") {
        return (value == true) ? 1.0 : 0.0;
    }
    if (typeName === "number") {
        return parseFloat(value);
    }
    if (typeName === "string") {
        if (!isNaN(value))
            return parseFloat(value);
    }
    throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "Double".');
};

ss_Convert.toString = function Convert$toString(value) {
    var typeName = typeof value;
    if (typeName === "string") {
        return value;
    }
    throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "String".');
};

ss_Convert.toInt32 = function Convert$toInt32(value) {
    var typeName = typeof value;
    if (typeName === "boolean") {
        return (value == true) ? 1 : 0;
    }
    if (typeName === "number") {
        return parseInt(value);
    }
    if (typeName === "string") {
        if (!isNaN(value))
            return parseInt(value);
    }
    throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "Int32".');
};

ss_Convert.toBoolean = function Convert$toBoolean(value) {
    var typeName = typeof value;
    if (typeName === "boolean") {
        return value;
    }
    if (typeName === "number") {
        return value > 0;
    }
    if (typeName === "string") {
        return value === "True" || value === "true";
    }
    throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "Boolean".');
};

ss_Convert.toChar = function Convert$toChar(value) {
    var typeName = typeof value;
    if (typeName === "string") {
        return value.charAt(0);
    }
    throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "Char".');
};

ss.Convert = ss_Convert;
ss.initClass(ss_Convert);
