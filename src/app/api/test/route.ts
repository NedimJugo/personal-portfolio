export async function GET() {
  return Response.json({ 
    message: "Your backend is working!", 
    timestamp: new Date().toISOString() 
  })
}