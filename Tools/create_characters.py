"""Original low-poly Touhou fan character models. Run with Blender --background --python."""
import bpy, math, os
from mathutils import Vector
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Characters')
os.makedirs(OUT, exist_ok=True)
SOURCE = os.path.join(ROOT, 'ArtSource')
os.makedirs(SOURCE, exist_ok=True)

def mat(name, color):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    return m

def part(name, location, scale, material, kind='sphere', vertices=16):
    if kind == 'cone':
        bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=1, radius2=.58, depth=2, location=location)
    elif kind == 'cylinder':
        bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=1, depth=2, location=location)
    elif kind == 'cube':
        bpy.ops.mesh.primitive_cube_add(size=2, location=location)
    else:
        bpy.ops.mesh.primitive_uv_sphere_add(segments=vertices, ring_count=8, radius=1, location=location)
    obj=bpy.context.object
    obj.name=name
    obj.scale=scale
    obj.data.materials.append(material)
    return obj

for who, palette in {
    'Traveler': ((.35,.55,.39),(.16,.21,.19),(.88,.73,.48)),
    'Nitori': ((.16,.43,.67),(.18,.62,.49),(.18,.42,.42)),
    'Aya': ((.88,.86,.77),(.11,.13,.18),(.69,.17,.16)),
    'Momiji': ((.92,.89,.81),(.90,.88,.83),(.71,.19,.19)),
}.items():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    cloth=mat('Cloth',palette[0]); hair=mat('Hair',palette[1]); trim=mat('Trim',palette[2])
    skin=mat('Skin',(.97,.77,.62)); black=mat('Ink',(.065,.08,.095)); white=mat('Cream',(.96,.94,.85))
    part('Dress',(0,0,.70),(.38,.27,.38),cloth,'cone')
    part('Torso',(0,0,1.08),(.29,.22,.28),cloth)
    part('Collar',(0,-.035,1.25),(.23,.18,.055),white,'cylinder')
    part('Head',(0,-.01,1.65),(.37,.31,.35),skin)
    part('HairCap',(0,.045,1.82),(.355,.30,.21),hair)
    for side in [-1,1]:
        part('HairSide',(side*.29,.06,1.52),(.10,.19,.28),hair)
        part('Fringe',(side*.12,-.248,1.84),(.17,.067,.13),hair)
        part('Eye',(side*.13,-.300,1.65),(.052,.013,.070),black)
        part('Iris',(side*.13,-.312,1.645),(.036,.007,.048),mat('Iris',(.21,.49,.52)))
        part('Pupil',(side*.13,-.320,1.65),(.020,.005,.035),black)
        part('EyeShine',(side*.14,-.326,1.675),(.012,.004,.017),white)
        part('Cheek',(side*.20,-.269,1.56),(.047,.012,.025),mat('Blush',(.94,.47,.45)))
        arm=part('Sleeve',(side*.34,0,1.05),(.12,.13,.23),cloth)
        arm.rotation_euler[1]=side*.2
        part('Hand',(side*.37,-.015,.88),(.09,.09,.10),skin)
        part('Leg',(side*.15,0,.27),(.08,.08,.23),white,'cylinder')
        part('Shoe',(side*.15,-.06,.08),(.115,.18,.08),black)
    part('Mouth',(0,-.3,1.54),(.035,.007,.012),trim)
    part('Belt',(0,0,.98),(.31,.24,.045),trim,'cylinder')
    part('DressHem',(0,0,.36),(.41,.28,.065),trim,'cylinder')
    part('RibbonCenter',(0,-.23,1.21),(.055,.035,.055),trim)
    for side in [-1,1]:
        bow=part('CollarRibbon',(side*.09,-.23,1.21),(.095,.035,.055),trim)
        bow.rotation_euler[1]=side*.35
    for i in range(12):
        a=i*math.pi*2/12
        part('SkirtPleat',(math.cos(a)*.34,math.sin(a)*.24,.61),(.028,.025,.22),cloth,'cube')
    if who=='Nitori':
        part('Cap',(0,.02,2.00),(.40,.33,.13),hair)
        part('CapBrim',(0,-.24,1.95),(.39,.22,.035),hair)
        part('Backpack',(0,.32,1.04),(.28,.14,.28),trim,'cube')
        part('Key',(0,-.25,1.11),(.045,.025,.085),mat('Gold',(.97,.74,.26)))
        for side in [-1,1]:
            part('TwinTail',(side*.38,.10,1.47),(.12,.16,.26),hair)
            part('TwinTailRibbon',(side*.34,.08,1.70),(.13,.13,.035),white)
        for i in range(2): part('Pocket',( (i-.5)*.28,-.265,.80),(.115,.02,.10),trim,'cube')
    elif who=='Aya':
        part('Tokin',(0,0,2.07),(.16,.13,.105),trim,'cube')
        for side in [-1,1]:
            part('PomPom',(side*.31,-.08,1.29),(.055,.055,.065),white)
        part('Camera',(.38,-.13,.91),(.11,.10,.085),black,'cube')
        part('Lens',(.38,-.23,.91),(.045,.025,.045),trim)
        for i in range(3): part('HairFeather',(.29+i*.03,.11,1.40-i*.11),(.12,.12,.18),hair)
        part('Skirt',(0,0,.70),(.40,.28,.26),black,'cone')
        part('Tie',(0,-.24,1.10),(.065,.035,.14),trim,'cone')
    elif who=='Momiji':
        for side in [-1,1]:
            ear=part('WolfEar',(side*.25,0,2.03),(.105,.09,.20),white,'cone')
            part('EarPink',(side*.25,-.075,2.035),(.062,.023,.12),trim,'cone')
        part('Tail',(0,.39,.68),(.14,.20,.38),white)
        part('Shield',(-.46,-.04,1.00),(.06,.26,.30),trim,'cylinder')
        part('Sword',(.45,0,.66),(.035,.035,.35),mat('Steel',(.61,.70,.73)),'cube')
        part('SwordGuard',(.45,0,.98),(.12,.08,.025),trim,'cube')
        part('WolfTailTip',(0,.41,.95),(.12,.15,.16),trim)
    else:
        part('StrawHat',(0,0,2.01),(.48,.41,.035),trim,'cylinder')
        part('HatTop',(0,0,2.10),(.28,.25,.10),trim,'cone')
        part('Satchel',(.28,.26,.94),(.16,.09,.16),trim,'cube')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,who+'.fbx'), use_selection=True,
        object_types={'MESH'}, add_leaf_bones=False, bake_anim=False, axis_forward='-Z',axis_up='Y')
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SOURCE,who+'.blend'))
print('CHARACTERS_READY',OUT)
