<!---
   Copyright 2018 Amazon

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->

# Script #

## We are an end to end solution
Developers tell us that their main needs in development are speed of development, reach to their customers, and tools to monetize their game. Amazon is uniquely positioned to help the developer, due to our strengths in client, cloud, community and commerce.

### Client
Lumberyard engine, with graphics, emotionfx animation, physx physics, script canvas cloud integration, component/entity/slice/eventbus model, editor UX.

### Cloud
Leveraging our strengths in cloud was the original motivation for connecting more deeply with game developers.

### Community
And now we have our offering in Twitch

### Commerce
AWS analytics help test changes to your deployed game to iterate to greater engagement from your players.

## Intro to GameLift
It's hard to write multiplayer games.  
1. Global deployment  
2. Reaching players with the least latency possible  
3. Making a scaleable deployment that is optimized for cost  
4. Making the system highly available and resilient  

Some of the things that you need to do to create such a system...  
1. Hire some network engineers  
2. Spend a good deal of time developing the

    - infrastructure management and scaling  
    - session management so the right players end up in the right games on the right infrastructure  
    - game session placement for global deployment  
    - managed matching service  

3. Make all the system parts redundant and stuff  
4. Debug all the weird scalability issues  

## Game Demo
And the service doesn't only work with Lumberyard (though it does work brilliantly with Lumberyard). This is a game sample that is build on Unity.

Starting two clients. I'm running the server locally so you can see it working. As I start the game on this client and start matching pairs and triplets, the score is increasing for this player. The input, in the form of matches is being sent to the server which authoritatively determines the correct state of the board in real time. The state of the board is sent to all connected clients at the same time.

## AWS Console Demo
Now of course, if I wasn't demoing live, then the server would be running on an instance in a GameLift fleet. A fleet is the 

So lets jump over to the AWS console and see how we would do that. In the dashboard we can see the three main constructs that represents








