	var iscountyselected = false;
	var previouscountyselected = "blank";
	var start = true;
	var past = null;
	var content_dir = "details";
	
	$(function(){
	
	var r = Raphael('map'),
	attributes = {
            fill: '#666',
            stroke: '#fff',
            'stroke-width':.5,
            'stroke-linejoin': 'round',
        },
	arr = new Array();
	
	for (var county in paths) {
		
		var obj = r.path(paths[county].path);
		
		obj.attr(attributes);
		
		arr[obj.id] = county;
			
							
		if(arr[obj.id] != 'blank') 
		{				
			obj.data('selected', 'notSelected');
			
			
		
			obj.node.id = arr[obj.id];
			
			obj.attr(attributes).attr( { title: paths[arr[obj.id]].name } );
			//edited by SB
			switch(paths[arr[obj.id]].bolge)
			{
				case '1': obj.attr(attributes).attr( { fill: 'tomato' } );break;
				case '2': obj.attr(attributes).attr( { fill: 'mediumseagreen' } );break;
				case '3': obj.attr(attributes).attr( { fill: 'orange' } );break;
				case '4': obj.attr(attributes).attr( { fill: 'dodgerblue' } );break;
				case '6': obj.attr(attributes).attr( { fill: 'violet' } );break;
			}
			/* if (paths[arr[obj.id]].bolge=='1')
				obj.attr(attributes).attr( { fill: 'blue' } ); */

			obj
			.hover(function(){
				$('#coatOfArms').addClass(arr[this.id]+'large sprite-largecrests');
				
				$('#countyInfo').text(paths[arr[this.id]].name);
				
				$('#searchResults').stop(true,true);
				
							
			}, function(){	
				$('#coatOfArms').removeClass();			
				if(paths[arr[this.id]].value == 'notSelected')
					{
					$('.'+paths[arr[this.id]].name)
							.slideUp('slow', function() { 
								$(this).remove(); 
							});
				}
			});
			$("svg a").qtip({
			
					content: {
						attr: 'title'
					},
					show: 'mouseover',
					hide: 'mouseout',
					position: {
						target: 'leave'
					},
					style: {
						classes: 'ui-tooltip-tipsy ui-tooltip-shadow',
						tip: false
					}
			});
			
			obj.click(function(){	
			
				if(paths[arr[this.id]].value == 'notSelected')
				{
						this.animate({
						fill: '#000'
					}, 200);
			
					paths[previouscountyselected].value = "notSelected";
					paths[arr[this.id]].value = "isSelected";
					
					previouscountyselected = paths[arr[this.id]].name;
					
					$('<div/>', {
							title: arr[this.id],
							'class': arr[this.id]+'small sprite-smallcrests'
						}).appendTo('#selectedCounties').qtip(countyCrest);
												
					$("#countymenu").val(paths[arr[this.id]].county); 
					
					
						
					if (!start && past != this)
					{
						past.animate({ fill: '#666'	}, 200);
						switch(paths[arr[past.id]].bolge)
					{
						
						case '1': past.animate({fill: 'tomato'}, 300);break;
						case '2': past.animate({fill: 'mediumseagreen'}, 300);break;
						case '3': past.animate({fill: 'orange'}, 300);break;
						case '4': past.animate({fill: 'dodgerblue'}, 300);break;
						case '6': past.animate({fill: 'violet'}, 300);break;
					}
					}
					past = this;
					start = false;					
				}
	
					
				else if(paths[arr[this.id]].value == 'isSelected')
					{
						this.animate({
							fill: '#15d4f5'
						}, 200);
						
						paths[arr[this.id]].value = "notSelected"; 
						
						$("." + previouscountyselected+'small').remove();
						
						
					}	
				
				});

			var countyCrest = 	{
					content: {
						attr: 'title'
					},
					position: {
						target: 'mouse'
					},
					style: {
						classes: 'ui-tooltip-tipsy ui-tooltip-shadow',
						tip: true
					}
			};
			
			function hoverin(e){
				if(paths[arr[this.id]].value == 'notSelected')
					this.animate({
						fill: '#15d4f5'}, 50);						
			}

			function hoverout(e){			
				if(paths[arr[this.id]].value == 'notSelected')
					
					//edited by SB
					switch(paths[arr[this.id]].bolge)
					{
						
						case '1': this.animate({fill: 'tomato'}, 300);break;
						case '2': this.animate({fill: 'mediumseagreen'}, 300);break;
						case '3': this.animate({fill: 'orange'}, 300);break;
						case '4': this.animate({fill: 'dodgerblue'}, 300);break;
						case '6': this.animate({fill: 'violet'}, 300);break;
					}
				
				// if (paths[arr[this.id]].bolge=='1')
					// this.animate({
						// fill: 'blue'}, 300);
						// else
							// this.animate({fill: '#666'}, 300);
			}

			obj.mouseout(hoverout);
				
			obj.mouseover(hoverin);

			$('#countyInfo').hide();
			
			$('#spinner').hide();
				
		}
		
	} 			
});