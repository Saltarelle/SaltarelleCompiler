///////////////////////////////////////////////////////////////////////////////
// TimeSpan

var ss_TimeSpan = function#? DEBUG TimeSpan$##() {
    this.ticks = 0;
    this.convertTicksToTime();
    this.convertTicksToTotalTime();
}

ss_TimeSpan.ticksPerMillisecond = 10000;
ss_TimeSpan.ticksPerSecond = 10000000;
ss_TimeSpan.ticksPerMinute = 600000000;
ss_TimeSpan.ticksPerHour = 36000000000;
ss_TimeSpan.ticksPerDay = 864000000000;

ss_TimeSpan.fromTicks = function#? DEBUG TimeSpan$fromTicks##(ticks) {
    var result = new ss_TimeSpan();
    result.ticks = ticks || 0;
    result.convertTicksToTime();
    result.convertTicksToTotalTime();
    return result;
};

ss_TimeSpan.fromValues = function#? DEBUG TimeSpan$fromValues##(days, hours, minutes, seconds, milliseconds) {
    var result = new ss_TimeSpan();
    result.ticks = (days || 0) * ss_TimeSpan.ticksPerDay + (hours || 0) * ss_TimeSpan.ticksPerHour +
        (minutes || 0) * ss_TimeSpan.ticksPerMinute + (seconds || 0) * ss_TimeSpan.ticksPerSecond +
        (milliseconds || 0) * ss_TimeSpan.ticksPerMillisecond;
    result.convertTicksToTime();
    result.convertTicksToTotalTime();
    return result;
}

ss_TimeSpan.prototype = {
    convertTicksToTime: function#? DEBUG TimeSpan$convertTicksToTime##() {
        this.days = parseInt(this.ticks / ss_TimeSpan.ticksPerDay);
        var diff = (this.ticks - ss_TimeSpan.ticksPerDay * this.days);
        this.hours = parseInt(diff / ss_TimeSpan.ticksPerHour);
        diff = (diff - ss_TimeSpan.ticksPerHour * this.hours);
        this.minutes = parseInt(diff / ss_TimeSpan.ticksPerMinute);
        diff = (diff - ss_TimeSpan.ticksPerMinute * this.minutes);
        this.seconds = parseInt(diff / ss_TimeSpan.ticksPerSecond);
        diff = (diff - ss_TimeSpan.ticksPerSecond * this.seconds);
        this.milliseconds = parseInt(diff / ss_TimeSpan.ticksPerMillisecond);
    },

    convertTicksToTotalTime: function#? DEBUG TimeSpan$convertTicksToTotalTime##() {
        this.totalDays = this.ticks / ss_TimeSpan.ticksPerDay;
        this.totalHours = this.ticks / ss_TimeSpan.ticksPerHour;
        this.totalMinutes = this.ticks / ss_TimeSpan.ticksPerMinute;
        this.totalSeconds = this.ticks / ss_TimeSpan.ticksPerSecond;
        this.totalMilliseconds = this.ticks / ss_TimeSpan.ticksPerMillisecond;
    },
    
    days: function#? DEBUG TimeSpan$days##() {
        return this.days;
    },

    hours: function#? DEBUG TimeSpan$hours##() {
        return this.hours;
    },

    minutes: function#? DEBUG TimeSpan$minutes##() {
        return this.minutes;
    },

    seconds: function#? DEBUG TimeSpan$seconds##() {
        return this.seconds;
    },

    milliseconds: function#? DEBUG TimeSpan$milliseconds##() {
        return this.milliseconds;
    },
    
    totalDays: function#? DEBUG TimeSpan$totalDays##() {
        return this.totalDays;
    },

    totalHours: function#? DEBUG TimeSpan$totalHours##() {
        return this.totalHours;
    },

    totalMinutes: function#? DEBUG TimeSpan$totalMinutes##() {
        return this.totalMinutes;
    },

    totalSeconds: function#? DEBUG TimeSpan$totalSeconds##() {
        return this.totalSeconds;
    },

    totalMilliseconds: function#? DEBUG TimeSpan$totalMilliseconds##() {
        return this.totalMilliseconds;
    },

    ticks: function#? DEBUG TimeSpan$ticks##() {
        return this.ticks;
    },
    
    compareTo: function#? DEBUG TimeSpan$compareTo##(other) {
        if (this.ticks < other.ticks)
            return -1;
        return this.ticks > other.ticks ? 1 : 0;
    },
    
    equals: function#? DEBUG TimeSpan$equals##(other) {
        return other != 'undefined' && other != null && this.ticks === other.ticks;
    },
    
    toString: function#? DEBUG TimeSpan$toString##() {
        var result = "";
        if(this.days >= 1 || this.days <= -1)
            if(this.days >= 9 || this.days <= -9)
                result += this.days + ".";
            else
                result += "0" + this.days + ".";
        
        if(this.hours >= 9 || this.hours <= -9)
            result += this.hours + ":";
        else
            result += "0" + this.hours + ":";
                                                         
        if(this.minutes >= 9 || this.minutes <= -9)
            result += this.minutes + ":";
        else
            result += "0" + this.minutes + ":";
                                                         
        if(this.seconds >= 9 || this.seconds <= -9)
            result += this.seconds;
        else
            result += "0" + this.seconds;

        if(this.milliseconds >= 1)
        {
            if(this.milliseconds <= 9)
                result += ".00" + this.milliseconds + "0000";
            else if(this.milliseconds <= 99)
                result += ".0" + this.milliseconds + "0000";
            else if(this.milliseconds <= 999)
                result += "." + this.milliseconds + "0000";
        }
        
        return result;
    }
};

ss.registerClass(global, 'ss.TimeSpan', ss_TimeSpan);