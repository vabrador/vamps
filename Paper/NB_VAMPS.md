Visualization and Analysis of Musculature Prediction in Simulation via Nonlinear Autoregressive Exogenous-Input Networks
======
*Nicholas Benson*  
*Massachusetts Institute of Technology*  
*6.UR Undergraduate Research, Spring 2016*

##Abstract

NARX networks operate on time-domain signal windows and output feedback windows to mimic the dynamics of a given system. I implement and test these networks to evaluate their feasibility for solving motion planning problems for a generic 18-muscle appendage analog developed in simulation using Unity, with an emphasis on visualizing the training and path response data to understand the potential and limits of the system as a whole. Ultimately, results point to some real potential, as even overfitted networks are able to return useful decision-making data about potential future output states, and the system appears ripe for network optimization and architectural experimentation.



##Introduction

Humans often perform tasks involving spatial musculature control without having to think about how to move each individual muscle. To reach out and touch an object in space, one need not understand the location and relative position effects of the deltoids, biceps and triceps brachii, and the like; the only required information is the position of the hand or fingertip relative to the desired end position, and with a high degree of accuracy, an unconscious system does the rest. Animals are also able to perform complicated motor tasks, such as balancing on a tree, galloping across a field, or manipulating objects in hand, presumably without significant thought as to which muscles to stiffen or relax (although we cannot ask them to be sure). Unconscious problem-solving systems like motion planning, including other problems like grasping, vision and alignment, and even merely walking offer a significant challenge to practical artificial intelligence and robotic systems that aim to operate in the real world.

One of the challenges of operating in the real world is cost and time; physical components and robotic systems require expertise and careful assembly, as well as plenty of physical working space and electronic components. In addition, finding real-world robotic analogs to biological muscle systems is prohibitively difficult for rapid development and testing. To facilitate the study of musculature control systems in an entirely virtual environment, I developed Balancer, a goal-oriented muscle control, sensory system and visualization platform using the Unity game engine. Its built-in simulated physics and API enables the collection and control of large amounts of physical simulation data, such as the length and contraction state of simulated muscles, as well as the real-time visualization of spatial information including position, velocity, and acceleration vectors; visualizations of predicted paths over time; and sensory data.

As the first application of Balancer, I conceived of and tested a system whereby data output by Balancer is used as input and output training data for a neural network in MATLAB. Once trained, the network weights are translated from MATLAB to Python and NumPy, which can receive and respond to network prediction requests sent in real-time from the Balancer simulation. Network response data is then used by one of Balancer's goal components to decide on the best solution to a specified muscle motion problem: moving the tip (or "nose") of a simulated appendage to an arbitrary point in space.



##Methods

###Creating a Musculature System in Unity

Musculature control systems necessarily operate within and must compensate for physical constraints such as gravity and the weight of the bodies such systems aim to control, so the development of such a system must happen in a physical simulation; either the real world (the one 'true' physical simulation) or a virtual one. There are many options for simulating classical mechanics, including bodies, collision effects, and spring forces in virtual space. These discrete-time, simplified-model physics simulations are often used to provide realistic visual effects in videogames, as they are designed to operate in real-time, at the cost of only being accurate within a scale of about two orders of magnitude. The Unity game engine, an industry-standard free game engine with a large and active community, offers physical simulation via Nvidia's PhysX engine. Utilizing such an engine reduces the problem of building and simulating an appendage with rigid bones, constrained joints, and musculature to expressing the structure of the appendage using the physical metaphors provided by the engine, including Rigidbodies, Colliders, Character Joints, and Spring Joints (see Appendix).

Existing creature simulation methods and character animation systems eschew simulating musculature in favor of a more direct approach: directly applying torques or rotations to joints in the simulated to animated body. By contrast, Balancer uses simulated springs as a simulated muscle analog. This means for Balancer compared to other creature simulations, the problem of estimating tip position is obfuscated because joint angles have been hidden, so sequentially-jointed-bone positions can no longer be calculated by simply summing the vectors defined by sequences of bone lengths and joint angles. This was an explicit design choice because organic systems don't have direct control over joint torques; rather, joint torques are mediated by musculature. Any logical transformations of data obtained from the musculature in order to obtain joint data, then, must be one of the tasks of a successful goal implementation in Balancer.

Unity simplifies simulating a musculature by providing bodies and springs, but two bodies connected by a spring does not quite fully describe the behavior of a muscle. When a muscle contracts, it both shortens and stiffens; the effective spring constant of the muscle increases, but this can only result in a change in the length of the muscle if the 'starting length' of the muscle (defined in terms of a spring) is shorter than the length of the muscle when it is fully relaxed. If the starting spring length is set to be the length of the muscle when relaxed, then increasing the spring constant of the muscle will stiffen but not shorten the muscle. This is solved by initializing the Spring Joint at a fraction of its intended rest distance and then pulling it out to that distance.

```C#
Vector3 initialPosition = proximalTendon.transform.position;
proximalTendon.transform.position += ((distalTendon.transform.position - proximalTendon.transform.position) * (7/8F));
// Initialize spring joint
spring = proximalTendon.rigidbody.gameObject.AddComponent<SpringJoint>();
spring.connectedBody = distalTendon.rigidbody;
spring.autoConfigureConnectedAnchor = false;
spring.anchor = proximalTendon.transform.localPosition;
spring.connectedAnchor = distalTendon.transform.localPosition;
defaultTendonDistance = (proximalTendon.transform.position - distalTendon.transform.position).magnitude;
spring.spring = 0f; // To be set in update
spring.maxDistance = 0;
spring.minDistance = 0;
spring.damper = 1f;
// Put the Transform of the proximal tendon back where it was.
proximalTendon.transform.position = initialPosition;
spring.anchor = proximalTendon.transform.localPosition;
```

Muscles as defined by Balancer also each request and receive memory registers for two floats as senses available to the simulated creature, indicating that muscle's current length (in Unity units, or meters), and that muscle's current contraction state, a dimensionless number from 0 to 10 that is converted every timestep into the muscle's spring constant. The appendage constructed for this application of Balancer defines 18 muscles, configured in a three-by-six hexagonal structure extending from a fixed-state body to a dynamic neck body and dynamic head body. The first six muscles connect the body to the neck, the second six muscles connect from the neck to the head, and the last six muscles are divided into a left-twist group and a right-twist group, where each group pulls and counter-pulls to apply a leftward or rightward rotation to the neck, rather than pulling the neck down to the left or right. Accordingly, these muscles are referred to in groups of six as "Body->Neck Muscles," "Neck->Head Muscles," or "Neck->Head Twist Muscles" (Figure 1).

    [IMAGE: Figure 1: Selecting the Muscle objects in the scene reveals the structure enabling the musculature to function.]
    ![Figure 1](https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png "Logo Title Text 1")


###Starting Out with Neural Networks

Neural networks are trained to solve problems by using large amounts of exemplary input/output data and iteratively modifying the nodes in the network (each acting as a transformation upon its input in some way) to optimize for its overall response to match the training data. Once trained, such networks have the advantage of a vanishingly small footprint compared to the training data, and speed. In an ideal scenario, these networks may "generalize" based on some patterns in the input data to maintain correctness even for input that lay outside the training input space. In the worst case, networks may "overfit" to their training data, and rapidly (even counter-intuitively) lose correctness very quickly when presented with input data close to but nevertheless outside the input space on which the network was trained.

Given a goal to move a tracked point attached to a body to a given location, the task of a muscle control system is to choose at any given timestep which contraction states should be assigned to each muscle to attain that goal. This requires knowing how far away the point currently is from the goal, in what direction the goal is from the point, and most importantly, how any given contraction of a muscle is going to affect the future position of the tracked point. What's more, these inputs affect the output position interdependently; one contraction state may cancel the effect of another, or two muscles may have a collectively different result on the output position than the mere summation of their effects on the point independently. For 18 (and potentially even more) muscles in a practical simulation, this is a difficult problem to assign a real-time system. Of course, the physics simulation that generates the training data in the first place can answer all of our questions in real time, but this is surely not how organic systems solve their motion planning problems. The goal is to find a cheaper, faster method for learning to predict future states, without relying on a description of the formal, physical structure musculature.


###Applying Neural Networks to Muscle Prediction

The task of the neural network in Balancer's muscle control system is to convert Nx18 muscle contraction states, 3x18 past muscle contraction states, and 3 past 3-vector states into N vectors describing the predicted future nose position states over time. In other words, given N timesteps-worth of imagined muscle states and the most recent muscle states and nose states, we expect the network to respond with N timesteps-worth of nose states that should correspond to the imagined muscle states.

The networks were expressed as NARX networks in MATLAB. NARX networks, short for Nonlinear AutoRegressive with eXogenous input networks, are neural networks with outputs that feeds back into its input with delays (autoregressive) alongside a separate input feed with delays (exogenous input). These networks can be constructed with a variety of hidden layer sizes and delay sizes; for this project, networks were generally constructed with hidden layer sizes ranging from 2 to 8, and delay sizes ranging from 2 to 4. Beyond those sizes, with 18 inputs, network training and construction would take inconveniently long due to computation and time limitations. A few network types were selected for the minimum hidden-layer and delay-state size that could match the training data to a mean squared error order around the magnitude of 10^-5.

* "v5" network:  
Input:  Muscle Contraction, 18-vector  
Output: Nose Position, 3-vector  
* "v6CL" network:  
Input:  Muscle Contraction, 18-vector  
Output: Muscle Length, 18-vector  
* "v6LN" network:  
Input:  Muscle Length, 18-vector  
Output: Nose Position, 3-vector  
* "v7CL" network:  
Input:  Muscle Contraction, 18-vector, Rigidbody motion restricted to 2D plane  
Output: Muscle Length, 18-vector  
* "v7LN" network:  
Input:  Muscle Length, 18-vector  
Output: Nose Position, 3-vector, Rigidbody motion restricted to 2D plane

This means there were three types of NARX networks used for this project. The first was trained to directly translate between muscle contraction inputs and nose position outputs. However, the original intention, later developed into the next two networks, was to further split them into two: one to process contraction states into expected length states (CL), and the other to convert muscle length states into nose position states (LN). 



###Implementing Network Training and Prediction

The data described in the previous section was obtained through a simple script that checks the state of each of the creature's muscles every fixed update of the Unity simulation. So-called "fixed" updates do not actually occur at a fixed interval in time, but Unity provides a FixedUpdate method abstraction that approximates fixed-interval script executions. The time interval between each execution works out to approximately ~20 milliseconds. The script collects a buffer of muscle states and periodically writes the buffer as CSV data to the filesystem. With this scheme, it is possible to collect large amounts of training data in parallel, by building and simultaneously running multiple instances of the creature simulation scene.

To get quickly up and running with network training, I decided to use MATLAB's neural network toolbox for constructing and testing a basic NARX network architecture. Once the training data has been written to the filesystem, it is loaded into MATLAB's memory and can be used for training. For this project, the possible range of network hidden and delay layer sizes were restricted due to construction and training time limitations and generally trained for 3 to 12 hours depending on the network size. The potential of the network to train effectively on the provided data was first evaluated within MATLAB by comparing actual, simulated response data to network-predicted response data; this was used to test various initial network configurations looked promising initially (Figure 2).

    [IMAGE: Figure 2: (v3seq_ctr_to_len_results.png)] Early testing results were performed for predicted future muscle length given muscle contraction states as input. The first two graphs show the actual and network-predicted responses for the same contraction input over time, and the graphs further down show more imagined response states from the network over time, given fake zeros on the contraction input. Maximum contraction values for upward-oriented muscles are switched on by the time the sequence reaches the second row from the bottom, showing how the network expects muscle lengths to change under those circumstances.]

While these results were promising, MATLAB plots were relatively difficult to use to gain insights and evaluate network performance. But I hypothesized that even an overfitted network may be able to provide useful responses if the nature of its responses were just correct enough to provide directional clues as a predictive heuristic for future nose positions. I continued the implementation of these types of networks as predicting modules under the hypothesis that response data of this sort would be useful even if the networks were overfitting to the training dataset.

To get a better sense of what the network is actually returning, the data being produced needed to be sent back to Unity for visualization. To do this, and to gain a better understanding of the transformations happening within the network, I hand-converted MATLAB's generated neural network function into a Python script, using NumPy to handle the necessary matrix calculations. Another, simpler script enables the conversion of MATLAB-exported network weights directly into Python objects that can be used to instantiate a working Python network implementation. Combining this with a Python-OSC server and a custom OSC subset implementation in Unity/C# (Note 1), I was able to achieve this bidirectional streaming, enabling further iteration and the visualizations in the next section.

*Note 1. Open Sound Control, a lightweight specification originally designed for electronic musical instruments that was repurposed as simple data transfer layer for this project.*


###Visualizing Training Data and Response Paths

This project incorporates two primary means of data visualization to help in the evaluation of network performance. A Memory Visualizer component reconstructs the values of all of the creature's registered sense in sequence in a time-aligned, frame-updated circular buffer. This visualization does not incorporate any network response data, but rather offers a way of visually understanding the sort of information the network is being trained on (Figure 3a). For visualizing the network response, a response processing component spawns a sphere that follows the path of Vector3s whenever such a path is returned by the network (Figure 3b).

    [Figure 3. An introduction to the visualizations that will be presented in Results. (a) Image: Sense visualization for the creature via the Memory Visualizer. Senses are aligned along the Y-axis, sense values shift individual cubes along the X-axis, and these values are kept and visualized down the Z-axis over time (in this case, up to 255 timesteps in the past). In this visualization, color maps to the kind of sense: Red squares are contraction states, blue squares muscle length states, and the green squares are the three components of the nose's position in 3-space. (b) Image: Colors have different meaning for path response visualizations, which are traced out over time from the nose. The blue spheres represent the actual network responses being returned for the muscle state in the right panel, which can be manually adjusted. Each prediction asks for 70 timesteps (~1.4 seconds) of predictive response data. When a prediction path response is received, a blue sphere is spawned and follows the path. Green and red spheres are used to show aligned difference paths, which are used to compensate for the effects of drift (and are discussed further in Results).]



##Results


###Visualizations Clarify the Training Data

To understand what the neural network is expected to learn, it's helpful to visualize the creature's senses disembodied and decontextualized in the same way that they will be presented to the network as training data. To this end, I constructed a Memory Visualizer on top of a simple sense-data accumulator (a Memory Accumulator) designed to store an arbitrary number of timesteps' worth of sense data for every sense registered with the creature.

Networks were trained on two types of input sequences: one hard-coded sequence involving a variety of smooth curvatures applied to varying combinations of muscles, and another that modifies each muscle state randomly every step up to a maximum change amount that is hooked up to a slow (~0.1 Hz) gain oscillation between 0 and 1. These training paradigms are shown in Figures 4a and 4b, along with the corresponding sense visualizations produced by the Memory Visualizer.

    [Movie: Figure 4. (a) The "gamut" sequence, a hard-coded sequence designed to capture a handful of smooth motions and unique motions, was used at the beginning of each training session. (b) After the gamut sequence, training resorts to a drunken walk-like algorithm for each contraction state that oscillates slowly between faster and slower muscle changes.]


###Each Network's Idle Response Exhibits an Unexpected Drift

Intuitively, if the creature is holding still by maintaining an unchanging contraction state, we would expect a well-functioning predictor to return no movement if we input the same contraction state. For each network that was trained, however, most contraction states' predicted path responses exhibited a very noticeable drift, almost always towards the center of the creature's range of movement. This may be because the training data doesn't cover idling for every possible muscle configuration, even though it swings around through most of them. Figure 5 shows the effect more clearly by showing idle state responses with the head capsule hidden.

    [Figure 5. (a, b) With the capsule renderer turned off for the head component, it's easier to see the effects of drift in various idle states. These paths don't predict any useful information; any distance from the creature's nose is intuitively incorrect because the creature is already maintaining the contraction state it is attempting to predict out further via the network. (c) Drift also occurs in the responses an earlier network configuration that converted contraction states directly into nose position paths, but in straight lines rather than arcs. (d) Here the movement of the creature during training and prediction is restricted to a 2D plane, reducing the dimensionality of the necessary prediction, but doing little to reduce drifting.


###Aligned Difference Paths Help Alleviate Drift

Even though the trained networks exhibit a problematic idle drift, they do still smoothly correlate their output path responses' general direction with changes in input contraction states. Subtracting imagined path responses from the idle path responses they began from, then, reveals the information we're actually looking for. In addition, because idle responses aren't refreshed constantly, we align the beginning node of each path before the element-wise subtraction occurs, so that the aligned difference paths don't reflect any potential offset from one another due to differing initial nose positions between the two prediction requests. In performing the calculation, some information is lost, and we can certainly no longer expect to get accurate paths, but we can get much more clear directional insight as to the consequences of a given change in contraction state (Figure 6).

    [Figure 6. To try to clear out some of the impact drift may have on use of prediction data as a heuristic, aligned difference paths represent the relative changes of network path responses between the idle (no change in contraction state) path response and the currently imagined contraction state's path response. In the visualization, the red sphere is interpreted as the idle state path response, and is subtracted from the raw path response (blue) in order to get the green spheres, which represent the aligned difference path.]


###Aligned Difference Paths Provide Scoring Information for a Simple Stochastic Search on Possible Muscle Actions

Once we have the ability to effectively query the network for the effect of a possible change in contraction state, the simplest way to put it to work in motion problem solving is by using it to score randomly-sampled imaginary contraction states based on the paths we can predict from them and choosing the highest-scoring result. In Figure 7, I demonstrate one implementation of this idea to test the network's ability to guide the creature to the location of a pink ball.

    [Figure 7. The creature's goal is to get its nose inside the pink ball. Specifically, it seeks to minimize the distance between the 3-vector position of the center of the pink ball and the 3-vector position of its nose. To do this, the creature queries the network with completely random contraction input states. Whenever it receives a prediction back, it measures the distance between the last 3-vector in the aligned difference path of the path response (measured from its nose) and the position of the ball. Effectively, that's the predicted position of the nose 1.4 seconds after the imagined contraction state is assigned. If the distance is less in Unity units (its nose is a one-unit cube) than the time it has been randomly querying the network in seconds, it chooses the muscle state that resulted in that path response, and a new round begins. By matching the scale of the pink ball with the time we have waited in seconds, visually, this means that angreen sphere will activate the creature's muscle state decision if its last position ever intersects with the pink sphere. We can visually judge how successful the creature's choice (and the network's prediction) was, then, by how close its nose comes to rest relative to the position of the pink ball when the round ended.]

There are a multiple rounds in Figure 7 that demonstrate a problem more difficult to work around than drift. There are many positions where stochastic contraction predictions all appear to exhibit very constricted response paths even though other path solutions exist away from a given position. This is most obvious when the shadows cast by the stochastic path responses line up along one axis.  These positions may represent holes in the training data, or a limitation of the architecture of the network to properly map the input to the output space. It is unclear to the author whether more data is an easy answer to this problem.



##Conclusions


Training time prevents testing out new network architectures from being easily iterable. But a solution relying on the NARX network for correct, fast predictive output has to involve a focus on network optimization and experimentation. Based on the presence of drift and wild inaccuracies from trapped positions, I was unable to get networks to effectively generalize upon their input data. The network responses did contain usable information for motion planning, so it's conceivable that these problems could be solved with patience and a much larger volume of data. Yet still, the logical next step in developing network-based solutions for Balancer would be to experiment with more intricate network architectures and methods, such as those used in deep learning.

One of the biggest future steps for this project as it pertains specifically to neural networks would be the incorporation of network training and implementation within the Unity build itself, rather than having to rely on local network data transfer and a separate Python network implementation. Not only would it significantly increase the potential search rate, it opens the door for a simulation that continuously trains and tests networks for solving a motion planning problem. The NARX network for this project has been used as a one-step solution to assigning a score to a contraction state, but it is almost certain that motion planning will require multiple layers of reasoning in the same way that many other AI problems seem to.

In conclusion, to investigate embodied spatial reasoning and motion planning as an artifical intelligence problem, I:
* Created a simulated 18-muscle creature appendage using Rigidbodies and springs as muscles in Unity.
* Trained NARX neural networks in a variety of configurations using simulation data of the musculature and the location of the appendage tip (the "nose").
* Deployed a Python network simulation server to enable remote querying of creature muscle state responses from networks trained in MATLAB.
* Rendered spatial information about the creature, network, and surrounding control system in space and over time to gain insight into the potential and limitations of the system as a whole.
* Designed a control system utilizing network-simulation response data to direct an 18-muscle appendage to a specific point in space in real-time.
* Demonstrated that a network trained on muscle input data can learn its input space well enough to have the potential for spatial problem-solving.


##APPENDIX: PhysX Object Types used by Balancer
"Rigidbody" - Rigidbodies are point-masses that have velocity and angular velocity and can experience forces. By default, they do not collide with one another; volumes where collisions can occur are expressed as Colliders, which will not move unless attached to a Rigidbody. Rigidbody Unity reference: http://docs.unity3d.com/ScriptReference/Rigidbody.html  
"Collider" - Colliders are bounded volumes that can detect intersections with other bounded volumes. Balancer utilizes SphereColliders, CapsuleColliders, and BoxColliders. Collider Unity reference: http://docs.unity3d.com/ScriptReference/Collider.html  
"Character Joint" - Character Joints are ball-and-socket-like joints most commonly used for the joints of animated characters in games. A Rigidbody attached to another Rigidbody by such a joint can swing along two axes and twist on a third. These axes can be constrained to mimic the limitations of, for example, a shoulder or elbow joint. Character Joint Unity reference: http://docs.unity3d.com/ScriptReference/CharacterJoint.html  
"Spring Joint" - Spring Joints aren't really like joints, except for the fact that they constrain two Rigidbodies. Two rigidbodies connected by a Spring Joint will act as though they are attached by a spring; they will experience forces according to Hooke's law. The spring constant is configurable in real-time. Spring Joint Unity reference: http://docs.unity3d.com/ScriptReference/SpringJoint.html  