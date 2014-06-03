var Rate = function (obj) {
	this.Name = obj.Name;
	this.Logo = obj.Logo;
	this.BankCode = obj.BankCode;
	this.LastUpdDt = obj.LastUpdDt;
	this.IsOutDated = obj.IsOutDated;
	this.Buy = obj.Buy;
	this.BuyCalc = ko.observable(obj.Buy);
	this.Sell = obj.Sell;
	this.SellCalc = ko.observable(obj.Sell);
	this.BuyDiff = obj.BuyDiff;
	this.SellDiff = obj.SellDiff;
};

var MainViewModel = function () {
	var self = this;
	self.rates = ko.observableArray();
	self.sortTypes = ["name", "buy", "sell"];
	self.sortType = self.sortTypes[1];
	self.sortDirection = -1;
	self.currencies = ["USD", "EUR"];
	self.currency = self.currencies[0];
	self.cbRates = ko.observableArray();
	self.convertValue = ko.observable();
	self.convertResults = ko.observableArray();
	self.isInited = false;

	self.sort = function (obj, event) {
		//hack for IE
		if (!event) event = window.event;
		//gets link's id to determine which column to sort
		var choseType = $(event.currentTarget)[0].id;
		//if link was clicked twice - change direction of sorting
		if (self.sortType == choseType)
			self.sortDirection *= -1;
		else {
			self.sortDirection = -1;
			self.convertValue("");
		}
		self.sortType = choseType;
		$("#sort-icon").remove();
		var sortIcon = self.sortDirection > 0
			? ' <i id="sort-icon" class="icon-chevron-up"></i>'
			: ' <i id="sort-icon" class="icon-chevron-down"></i>';
		$("#" + self.sortType).after(sortIcon);
		self.exChangeDir();
		self.getRates();
	};
	self.getRates = function () {
		self.rates([]);
		$("#main-rtable tbody").after("<tr><td colspan='4'><img src='/images/loading.gif'></td></tr>");
		var sortBy = (self.sortTypes.indexOf(self.sortType) + 1) * self.sortDirection;
		$.post("/rate/gettoprates", { Currency: self.currency, City: erm.city().Value, SortBy: sortBy }, function (data) {
			$("#main-rtable tr:last").remove();
			ko.utils.arrayForEach(data, function (item) {
				self.rates.push(new Rate(item));
			});
			$('#main-rtable tbody tr').click(erm.bvm.showBank);
		});
	};
	self.getDate = function () {
		var now = new Date();
		var dd = now.getUTCDate();
		if (dd < 10) dd = '0' + dd;
		var mm = now.getUTCMonth() + 1;
		if (mm < 10) mm = '0' + mm;
		var yy = now.getUTCFullYear();

		return dd + "." + mm + "." + yy;
	};
	self.changeCur = function () {
		var $parent = $('#cur-toggle');
		$parent && $parent
			.find('.active')
			.removeClass('active btn-danger');
		$(this).toggleClass('active btn-danger');
		//prevent changing currency from clicking on the same cur
		if (self.currency == $(this).data('cur')) return;
		self.currency == self.currencies[0]
			? self.currency = self.currencies[1]
			: self.currency = self.currencies[0];
		setCookie("cur_code", self.currency, { expires: new Date(2035, 10, 20) });
		self.exChangeDir();
		//don't change convert value if it is roubles for buying values
		if (self.sortType != self.sortTypes[2])
			self.convertValue("");
		else {
			var temp = self.convertValue();
			self.convertValue(0);
			self.convertValue(temp);
		}
		self.getRates();
	};
	self.getCbRates = function () {
		$.post("/rate/getcbrates", {}, function (data) {
			self.cbRates(data);
		});
	};
	self.exChangeDir = function () {
		if (self.sortType == self.sortTypes[2])
			$('.exchange-form').css('background-image', 'url("/images/RUB.jpg")');
		else $('.exchange-form').css('background-image', 'url("/images/' + self.currency + '.jpg")');
	};
	self.calc = ko.computed(function () {
		var sum = self.convertValue(),
			rates = self.rates.peek();
		var moneyRegExp = /^(?=.)(?!0\d)\d*((\.|\,)\d+)?$/;
		if (typeof sum === "undefined" || !sum.match(moneyRegExp))
			sum = 1;
		else sum = sum.replace(',', '.');
		ko.utils.arrayForEach(rates, function (item) {
			if (self.sortType == self.sortTypes[1]) {
				item.BuyCalc(item.Buy * sum);
				item.SellCalc(item.Sell);
			} else if (self.sortType == self.sortTypes[2]) {
				item.BuyCalc(item.Buy);
				typeof sum === "number"
					? item.SellCalc(item.Sell)
					: item.SellCalc(sum / item.Sell);
			}
		});
	}).extend({ throttle: 400 });

	self.init = function () {
		if (self.isInited) {
			self.getRates();
			return;
		}
		self.isInited = true;
		self.getCbRates();
		$('span#date').text(self.getDate());
		$('div#cur-toggle button').click(self.changeCur);
		//set chosen currency
		var cookCur = getCookie("cur_code");
		if (cookCur != self.currency && cookCur != undefined)
			self.currency = cookCur;
		self.exChangeDir();
		$('#cur-toggle button[data-cur="' + self.currency + '"]').trigger('click');
		self.getRates();
	};
};