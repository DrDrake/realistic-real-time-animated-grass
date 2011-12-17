package
{
	import flash.display.Sprite;
	import flash.display.MovieClip;
	import flash.events.MouseEvent;
	import fl.controls.*;
	import flash.text.*;
	import flash.text.TextFieldType;
	import flash.events.*;
	import flash.media.Sound;
	import flash.net.*;
	
	public class MainSleep extends MovieClip
	{
		private var classes:Array = new Array("Möbel", "Technik", "Bettzeug", "Kleidung", "Kuscheltier");
		private var classCounts:Array = new Array(0, 0, 0, 0, 0);
		private var index:Number = new Number();
		private var currentClass:String = new String();
		private var targetLabel:Label = new Label();
		private var pointLabel:Label = new Label();
		private var currentitem:MovieClip = null;
		private var oldItemPosX:int;
		private var oldItemPosY:int;
		private var points:int = 0;
		
		//audio
		var indexOkay:Number;
		var indexWrong:Number;
		
		var okaySound1:Sound = new Sound();
		var okaySound2:Sound = new Sound();
		var okaySound3:Sound = new Sound();
		var okaySound4:Sound = new Sound();
		private var okaySounds:Array = new Array();
		
		var wrongSound1:Sound = new Sound();
		var wrongSound2:Sound = new Sound();
		private var wrongSounds:Array = new Array();
		
		public function MainSleep() 
		{
			//audio
			okaySound1.load(new URLRequest("Sounds/011-04.mp3"));
			okaySound2.load(new URLRequest("Sounds/011-05.mp3"));
			okaySound3.load(new URLRequest("Sounds/011-07.mp3"));
			okaySound4.load(new URLRequest("Sounds/011-03.mp3"));
			okaySounds.push(okaySound1);
			okaySounds.push(okaySound2);
			okaySounds.push(okaySound3);
			okaySounds.push(okaySound4);
			
			wrongSound1.load(new URLRequest("Sounds/011-06.mp3"));
			wrongSound2.load(new URLRequest("Sounds/011-08.mp3"));
			wrongSounds.push(wrongSound1);
			wrongSounds.push(wrongSound2);
			
			var xPos:int = 10;
			var yPos:int = 10;
			
			dropWait.x = xPos;
			dropWait.y = yPos;
			
			//dropWait.visible = false;
			dropOkay.x = xPos;
			dropOkay.y = yPos;
			dropOkay.visible = false;
			
			dropFail.x = xPos;
			dropFail.y = yPos;
			dropFail.visible = false;
			
			//Classes for droping
			addChild(targetLabel);
			targetLabel.autoSize = TextFieldAutoSize.CENTER; 
			targetLabel.x = 55; 
			targetLabel.y = 15;
			targetLabel.name = "targetLabel";
			setNewClassOnLabel();
			
			//Points
			addChild(pointLabel);
			pointLabel.autoSize = TextFieldAutoSize.CENTER; 
			pointLabel.x = 760; 
			pointLabel.y = 15;
			var str:String = new String('<font size="30">' + points + ' x</font>');
			pointLabel.htmlText = str;
			pointLabel.parent.setChildIndex( pointLabel, pointLabel.parent.getChildIndex( pointLabel ) - 1);
			
			pointMove.stop();
			pointMove.visible = false;
			
			attachListener();
		}
		
		private function mouseDown(e:MouseEvent):void 
		{
			var item:MovieClip = MovieClip(e.target);
			oldItemPosX = item.x;
			oldItemPosY = item.y;
			item.startDrag();
			currentitem = item;
		}
		
		private function mouseUp(e:MouseEvent):void 
		{
			currentitem.stopDrag();
			currentitem.x = oldItemPosX;
			currentitem.y = oldItemPosY;
			currentitem = null;
		}
		
		private function mouseUpLabel(e:MouseEvent):void 
		{
			if(currentitem != null)
			{
			
				var lastIndexOkay:Number = indexOkay;
				var lastindexWrong:Number = indexWrong;
				
				indexOkay = Math.floor(Math.random()* okaySounds.length);
				indexWrong = Math.floor(Math.random()* wrongSounds.length);
				
				if(indexOkay == lastIndexOkay)
				{
					indexOkay = (indexOkay + 1)% okaySounds.length;
				}
				
				if(indexWrong == lastindexWrong)
				{
					indexWrong = (indexWrong + 1)% wrongSounds.length;
				}
				
				
				currentitem.stopDrag();
				
				if(currentitem.objName == currentClass)
				{
					okaySounds[indexOkay].play();
					
					currentitem.visible = false;
					
					dropFail.visible = false;
					dropWait.visible = false;
					dropOkay.visible = true;
					dropOkay.gotoAndPlay(0);
					dropOkay.addEventListener(Event.ENTER_FRAME, checkLastFrameDropColor);
					
					decreaseClassCount(currentitem);
					setNewClassOnLabel();
					trace(classes);
					pointMove.visible = true;
					pointMove.addEventListener(Event.ENTER_FRAME, checkLastFramePoints);
					pointMove.gotoAndPlay(0);
				}
				else if(currentitem.objName != currentClass)
				{
					wrongSounds[indexWrong].play();
					
					dropWait.visible = false;
					dropOkay.visible = false;
					dropFail.visible = true;
					dropFail.gotoAndPlay(0);
					dropFail.addEventListener(Event.ENTER_FRAME, checkLastFrameDropColor);
					currentitem.visible = false;
					decreaseClassCount(currentitem);
					trace(classes);
				}
				//Check for finish
				if(checkFinish())
				{
					var str:String = new String('<font size="30">Fertig!</font>');
					targetLabel.htmlText = str;
					dropWait.visible = false;
					dropOkay.visible = true;
					dropFail.visible = false;
					dropOkay.gotoAndStop(0);
				}
				currentitem = null;
			}
		}
		
		function decreaseClassCount(item:MovieClip):void
		{
			switch (item.objName)
			{
				//"Möbel", "Technik", "Bettzeug", "Kleidung", "Kuscheltier"
				case "Möbel":
				classCounts[0]--;
				break;

				case "Technik":
				classCounts[1]--;
				break;

				case "Bettzeug":
				classCounts[2]--;
				break;
				
				case "Kleidung":
				classCounts[3]--;
				break;
				
				case "Kuscheltier":
				classCounts[4]--;
				break;
			}
			
			var i:int; 
			for(i = 0; i < classCounts.length; i++)
			{
				if(classCounts[i] == 0)
				{
					classes[i] = "null";
					classCounts[i] = -1;
				}
			}
			
		}
		
		function checkFinish():Boolean
		{
			var toReturn:Boolean = true;
			var i:int; 
			for(i = 0; i < classes.length; i++)
			{
				if(classes[i] != "null")
				{
					toReturn = false;
				}
			}
			return toReturn;
		}
		
		function checkLastFramePoints(e:Event):void
		{ 
			var item:MovieClip = MovieClip(e.target);
			if(item.currentFrame == item.totalFrames)
			{
				item.stop();
				item.visible = false;
				points++;
				var str:String = new String('<font size="30">' + points + ' x</font>');
				pointLabel.htmlText = str;
				item.removeEventListener(Event.ENTER_FRAME, checkLastFramePoints);
			}
		}
		
		function checkLastFrameDropColor(e:Event):void
		{ 
			var item:MovieClip = MovieClip(e.target);
			if(item.currentFrame == item.totalFrames)
			{
				dropWait.visible = true;
				dropOkay.visible = false;
				dropFail.visible = false;
				item.stop();
				item.removeEventListener(Event.ENTER_FRAME, checkLastFrameDropColor);
			}
		}
		
		private function setNewClassOnLabel():void
		{
			var notNull:Array = new Array();
			var i:int; 
			for(i = 0; i < classes.length; i++)
			{
				if(classes[i] != "null")
				{
					notNull.splice(notNull.length, 0, classes[i]);
				}
			}
			
			
			var lastIndex:Number = index;
			index = Math.floor(Math.random()* notNull.length);
			
			if(index == lastIndex)
			{
				index = (index + 1)% notNull.length;
			}
			
			currentClass = notNull[index];
			
			var str:String = new String('<font size="30">' + currentClass + '</font>');
			targetLabel.htmlText = str;
		}
		
		private function attachListener():void
		{
			targetLabel.addEventListener(MouseEvent.MOUSE_UP, mouseUpLabel);
			dropWait.addEventListener(MouseEvent.MOUSE_UP, mouseUpLabel);
			dropFail.addEventListener(MouseEvent.MOUSE_UP, mouseUpLabel);
			dropOkay.addEventListener(MouseEvent.MOUSE_UP, mouseUpLabel);
			
			stage.addEventListener(MouseEvent.MOUSE_MOVE, mouseMove);
			
			kommode1.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kommode1.objName = "Möbel";
			classCounts[0]++;
			
			kommode2.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kommode2.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			kommode2.objName = "Möbel";
			classCounts[0]++;
			
			kommode3.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kommode3.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			kommode3.objName = "Möbel";
			classCounts[0]++;
			
			schrank.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			schrank.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			schrank.objName = "Möbel";
			classCounts[0]++;
			
			bett.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			bett.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			bett.objName = "Möbel";
			classCounts[0]++;
			
			lcd.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			lcd.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			lcd.objName = "Technik";
			classCounts[1]++;
			
			wecker.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			wecker.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			wecker.objName = "Technik";
			classCounts[1]++;
			
			decke1.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			decke1.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			decke1.objName = "Bettzeug";
			classCounts[2]++;
			
			decke2.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			decke2.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			decke2.objName = "Bettzeug";
			classCounts[2]++;
			
			decke3.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			decke3.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			decke3.objName = "Bettzeug";
			classCounts[2]++;
			
			decke4.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			decke4.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			decke4.objName = "Bettzeug";
			classCounts[2]++;
			
			kissen1.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kissen1.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			kissen1.objName = "Bettzeug";
			classCounts[2]++;
			
			kissen2.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kissen2.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			kissen2.objName = "Bettzeug";
			classCounts[2]++;
			
			kissen3.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kissen3.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			kissen3.objName = "Bettzeug";
			classCounts[2]++;
			
			kissen4.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			kissen4.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			kissen4.objName = "Bettzeug";
			classCounts[2]++;
			
			mütze1.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			mütze1.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			mütze1.objName = "Kleidung";
			classCounts[3]++;
			
			mütze2.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			mütze2.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			mütze2.objName = "Kleidung";
			classCounts[3]++;
			
			mütze3.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			mütze3.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			mütze3.objName = "Kleidung";
			classCounts[3]++;
			
			pyjama1.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			pyjama1.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			pyjama1.objName = "Kleidung";
			classCounts[3]++;
			
			pyjama2.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			pyjama2.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			pyjama2.objName = "Kleidung";
			classCounts[3]++;
			
			panda1.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			panda1.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			panda1.objName = "Kuscheltier";
			classCounts[4]++;
			
			teddy.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			teddy.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			teddy.objName = "Kuscheltier";
			classCounts[4]++;
			
			ernie.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			ernie.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			ernie.objName = "Kuscheltier";
			classCounts[4]++;
			
			bert.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			bert.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
			bert.objName = "Kuscheltier";
			classCounts[4]++;
		}
		
		private function mouseMove(e:MouseEvent):void
		{
			var x:Number = e.stageX;
			var y:Number = e.stageY;
			
			if(x > stage.stageWidth || x < 1 || y > stage.stageHeight || y < 1)
			{
				//reset Label
				currentitem.x = oldItemPosX;
				currentitem.y = oldItemPosY;
				currentitem.stopDrag();
			}
			trace("x,y: " + x + "," + y)
		}
	}
}
