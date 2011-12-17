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
	
	public class MainWork extends MovieClip
	{
		private var words:Array = new Array(
											  "Möbel", "Computer", "Buch", "Stift", "Seitenanzahl", 
											  "Einband", "Farbe", "Holzart", "Abmessungen", "Prozessor", 
											  "Speicher", "lesen()", "schreiben()", "malen()", "aufbauen()", 
											  "abstauben()", "rechnen()", "einschalten()"
		);
		private var wordsFurniture:Array = new Array("Möbel", "Holzart", "Abmessungen", "aufbauen()", "abstauben()");
		private var wordsTechnique:Array = new Array("Computer", "Prozessor", "Speicher", "rechnen()", "einschalten()");
		private var wordsBooks:Array = new Array("Buch", "Seitenanzahl", "Einband", "lesen()");
		private var wordsPencils:Array = new Array("Stift", "Farbe", "schreiben()", "malen()");
		
		private var currentWord:String = new String();
		private var currentitem:TextField = null;
		
		private var index:Number = new Number();
	
		private var wordEqualsHit:int;
		
		private var dragLabel:Label = new Label();
		private var dragLabelMC:MovieClip = new MovieClip();
		
		private var typeLabel:Label = new Label();
		
		private var pointLabel:Label = new Label();
		private var oldItemPosX:int;
		private var oldItemPosY:int;
		private var points:int = 0;
		
		//audio
		var indexOkay:Number;
		var indexWrong:Number;
		
		var okaySound1:Sound = new Sound();
		var okaySound2:Sound = new Sound();
		var okaySound3:Sound = new Sound();
		private var okaySounds:Array = new Array();
		
		var wrongSound1:Sound = new Sound();
		var wrongSound2:Sound = new Sound();
		private var wrongSounds:Array = new Array();
		
		
		public function MainWork() 
		{
			//audio
			okaySound1.load(new URLRequest("Sounds/011-04.mp3"));
			okaySound2.load(new URLRequest("Sounds/011-05.mp3"));
			okaySound3.load(new URLRequest("Sounds/011-07.mp3"));
			okaySounds.push(okaySound1);
			okaySounds.push(okaySound2);
			okaySounds.push(okaySound3);
			
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
			
			oldItemPosX = 75;
			oldItemPosY = 15;
			
			addChild(dragLabelMC);
			dragLabelMC.x = oldItemPosX;
			dragLabelMC.y = oldItemPosY;
			
			
			dragLabelMC.addChild(dragLabel);
			dragLabel.visible = true;
			dragLabel.autoSize = TextFieldAutoSize.CENTER;
			dragLabel.name = "dragLabel";
			setNewWordOnLabel();
			
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
			dragLabelMC.startDrag();
			currentitem = TextField(e.target);
		}
		
		private function mouseUp(e:MouseEvent):void 
		{
			if(currentitem == null)
				return;
			
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
			
			dragLabelMC.stopDrag();
			wordEqualsHit = -10;
			checkHit(e);
			
			//Hit proper Item with the label
			if(wordEqualsHit > -1)
			{				
				okaySounds[indexOkay].play();
				words.splice(index, 1);
				trace(words);
				setNewWordOnLabel();
				pointMove.visible = true;
				pointMove.addEventListener(Event.ENTER_FRAME, checkLastFramePoints);
				pointMove.gotoAndPlay(0);
				
				dropFail.visible = false;
				dropWait.visible = false;
				dropOkay.visible = true;
				dropOkay.gotoAndPlay(0);
				dropOkay.addEventListener(Event.ENTER_FRAME, checkLastFrameDropColor);
				
				//Check for finish
				if(checkFinish())
				{
					var str:String = new String('<font size="30">Fertig!</font>');
					var dragLabel:Label = Label(dragLabelMC.getChildByName("dragLabel"));
					dragLabel.htmlText = str;
					dragLabelMC.removeEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
					dragLabelMC.removeEventListener(MouseEvent.MOUSE_UP, mouseUp);
					dropWait.visible = false;
					dropOkay.visible = true;
					dropFail.visible = false;
					dropOkay.gotoAndStop(0);
				}
			}
			//Didn't hit proper item 
			else if(wordEqualsHit == -1)
			{
				wrongSounds[indexWrong].play();
				words.splice(index, 1);
				trace(words);
				setNewWordOnLabel();
				
				dropWait.visible = false;
				dropOkay.visible = false;
				dropFail.visible = true;
				dropFail.gotoAndPlay(0);
				dropFail.addEventListener(Event.ENTER_FRAME, checkLastFrameDropColor);
			}
			//reset Label
			dragLabelMC.x = oldItemPosX;
			dragLabelMC.y = oldItemPosY;
		}
		
		private function checkHit(e:MouseEvent):void
		{
			var mouseX:Number = e.stageX;
			var mouseY:Number = e.stageY;
			
			if
			(
			 	stifte.hitTestPoint(mouseX, mouseY) ||
				stift1.hitTestPoint(mouseX, mouseY) ||
				stift2.hitTestPoint(mouseX, mouseY) ||
				stift3.hitTestPoint(mouseX, mouseY)
			)
			{
				trace("Hit: Pencils");
				wordEqualsHit = wordsPencils.indexOf(currentWord);  
			}
			else if
			(
				computer.hitTestPoint(mouseX, mouseY) ||
				calc.hitTestPoint(mouseX, mouseY) ||
				laptop.hitTestPoint(mouseX, mouseY) ||
				telefon.hitTestPoint(mouseX, mouseY)
			)
			{
				trace("Hit: Tech");
				wordEqualsHit = wordsTechnique.indexOf(currentWord);  
			}
			else if
			(
			 	book1.hitTestPoint(mouseX, mouseY) ||
				book2.hitTestPoint(mouseX, mouseY) ||
				book3.hitTestPoint(mouseX, mouseY) ||
				book4.hitTestPoint(mouseX, mouseY) ||
				book5.hitTestPoint(mouseX, mouseY) ||
				book6.hitTestPoint(mouseX, mouseY)
			)
			{
				trace("Hit: Books");
				wordEqualsHit = wordsBooks.indexOf(currentWord);  
			}
			else if
			(
			 	regal1.hitTestPoint(mouseX, mouseY) ||
				regal2.hitTestPoint(mouseX, mouseY) ||
			   	desk.hitTestPoint(mouseX, mouseY) ||
			   	couch.hitTestPoint(mouseX, mouseY) ||
			   	chair.hitTestPoint(mouseX, mouseY)
			)
			{
				trace("Hit: Furniture");
				wordEqualsHit = wordsFurniture.indexOf(currentWord);
			}
		}
			
		function checkFinish():Boolean
		{
			var toReturn:Boolean = false;
			
			if(words.length == 0)
			{
				toReturn = true;
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
				item.stop();
				item.removeEventListener(Event.ENTER_FRAME, checkLastFrameDropColor);
			}
		}
		
		private function setNewWordOnLabel():void
		{
			index = Math.floor(Math.random()* words.length);
			
			currentWord = words[index];
			
			var str:String = new String('<font size="30">' + currentWord + '</font>');
			var dragLabel:Label = Label(dragLabelMC.getChildByName("dragLabel"));
			dragLabel.htmlText = str;
		}
		
		private function attachListener():void
		{
			dragLabelMC.addEventListener(MouseEvent.MOUSE_DOWN, mouseDown);
			dragLabelMC.addEventListener(MouseEvent.MOUSE_UP, mouseUp);
		}
	}
}
