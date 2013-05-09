///////////////////////////////////////////////////////////////////////////////
// Convert

var ss_Convert = function Convert$() {
};

ss_Convert.toSingle = function Convert$toSingle(value) {
  var typeName = typeof value;
  if(typeName === "Object")
    typeName = ss.getTypeFullName(value);

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
  throw 'Invalid conversion of "' + typeName + '" to "Single".';
  //TODO throw new ss_InvalidCastException('Invalid conversion of "' + typeName + '" to "Single".');
};

ss.Convert = ss_Convert;
ss.initClass(ss_Convert);
